using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_CuaHangDienThoai.Data;
using QL_CuaHangDienThoai.Helpers;
using QL_CuaHangDienThoai.Models;
using QL_CuaHangDienThoai.ViewModels;

namespace QL_CuaHangDienThoai.Controllers
{
    [Authorize(Policy = "CustomerOnly")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Xem giỏ hàng
        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreateCart();
            var cartViewModel = await MapToCartViewModel(cart);
            return View(cartViewModel);
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var product = await _context.DienThoais.FindAsync(model.MaDT);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            if (product.SoLuongTon < model.SoLuong)
            {
                return Json(new { success = false, message = "Không đủ hàng trong kho" });
            }

            var cart = await GetOrCreateCart();
            var existingItem = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(ct => ct.MaGH == cart.MaGH && ct.MaDT == model.MaDT);

            if (existingItem != null)
            {
                // Cập nhật số lượng
                var newQuantity = existingItem.SoLuong + model.SoLuong;
                if (newQuantity > product.SoLuongTon)
                {
                    return Json(new { success = false, message = "Tổng số lượng vượt quá tồn kho" });
                }
                existingItem.SoLuong = newQuantity;
                _context.Update(existingItem);
            }
            else
            {
                // Thêm mới
                var cartItem = new ChiTietGioHang
                {
                    MaGH = cart.MaGH,
                    MaDT = model.MaDT,
                    SoLuong = model.SoLuong,
                    NgayThem = DateTime.Now
                };
                _context.ChiTietGioHangs.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            // Đếm số lượng items trong giỏ
            var totalItems = await _context.ChiTietGioHangs
                .Where(ct => ct.MaGH == cart.MaGH)
                .SumAsync(ct => ct.SoLuong);

            return Json(new
            {
                success = true,
                message = "Đã thêm vào giỏ hàng",
                cartCount = totalItems
            });
        }

        // Cập nhật số lượng
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(string maDT, int quantity)
        {
            if (quantity <= 0)
            {
                return await RemoveFromCart(maDT);
            }

            var cart = await GetOrCreateCart();
            var cartItem = await _context.ChiTietGioHangs
                .Include(ct => ct.DienThoai)
                .FirstOrDefaultAsync(ct => ct.MaGH == cart.MaGH && ct.MaDT == maDT);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Sản phẩm không có trong giỏ hàng" });
            }

            if (quantity > cartItem.DienThoai.SoLuongTon)
            {
                return Json(new { success = false, message = "Không đủ hàng trong kho" });
            }

            cartItem.SoLuong = quantity;
            _context.Update(cartItem);
            await _context.SaveChangesAsync();

            var newTotal = cartItem.ThanhTien;
            return Json(new { success = true, itemTotal = newTotal });
        }

        // Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(string maDT)
        {
            var cart = await GetOrCreateCart();
            var cartItem = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(ct => ct.MaGH == cart.MaGH && ct.MaDT == maDT);

            if (cartItem != null)
            {
                _context.ChiTietGioHangs.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // Xóa toàn bộ giỏ hàng
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var cart = await GetOrCreateCart();
            var cartItems = await _context.ChiTietGioHangs
                .Where(ct => ct.MaGH == cart.MaGH)
                .ToListAsync();

            _context.ChiTietGioHangs.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // Lấy số lượng items trong giỏ hàng (cho header)
        public async Task<IActionResult> GetCartCount()
        {
            var cart = await GetOrCreateCart();
            var count = await _context.ChiTietGioHangs
                .Where(ct => ct.MaGH == cart.MaGH)
                .SumAsync(ct => ct.SoLuong);

            return Json(new { count });
        }

        // Helper methods
        private async Task<GioHang> GetOrCreateCart()
        {
            var tenDangNhap = User.Identity.Name;
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.TenDangNhap == tenDangNhap);

            if (khachHang == null)
                throw new InvalidOperationException("Không tìm thấy thông tin khách hàng");

            var cart = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaKH == khachHang.MaKH);

            if (cart == null)
            {
                var cartId = await GenerateCartId();
                cart = new GioHang
                {
                    MaGH = cartId,
                    MaKH = khachHang.MaKH,
                    NgayTao = DateTime.Now
                };
                _context.GioHangs.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private async Task<CartViewModel> MapToCartViewModel(GioHang cart)
        {
            var cartItems = await _context.ChiTietGioHangs
                .Where(ct => ct.MaGH == cart.MaGH)
                .Include(ct => ct.DienThoai)
                .ToListAsync();

            var cartViewModel = new CartViewModel
            {
                MaGH = cart.MaGH,
                Items = cartItems.Select(ct => new CartItemViewModel
                {
                    MaDT = ct.MaDT,
                    TenDT = ct.DienThoai?.TenDT ?? "",
                    HinhAnh = ImageHelper.GetProductImage(ct.MaDT),
                    DonGia = ct.DienThoai?.DonGia ?? 0,
                    SoLuong = ct.SoLuong,
                    SoLuongTon = ct.DienThoai?.SoLuongTon ?? 0
                }).ToList()
            };

            return cartViewModel;
        }

        private async Task<string> GenerateCartId()
        {
            var lastCart = await _context.GioHangs
                .OrderByDescending(g => g.MaGH)
                .FirstOrDefaultAsync();

            if (lastCart == null)
                return "GH001";

            var lastNumber = int.Parse(lastCart.MaGH.Substring(2));
            return $"GH{(lastNumber + 1):D3}";
        }
    }
}