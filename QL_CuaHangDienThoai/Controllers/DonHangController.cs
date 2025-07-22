using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_CuaHangDienThoai.Data;
using QL_CuaHangDienThoai.Models;
using QL_CuaHangDienThoai.ViewModels;

namespace QL_CuaHangDienThoai.Controllers
{
    [Authorize]
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách đơn hàng cho admin/nhân viên
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Index()
        {
            var donHangs = await _context.HoaDons
                .Include(d => d.KhachHang)
                .Include(d => d.QuanTriVien)
                .OrderByDescending(d => d.NgayLap)
                .ToListAsync();

            return View(donHangs);
        }

        // Đơn hàng của khách hàng
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> MyOrders()
        {
            var tenDangNhap = User.Identity.Name;
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.TenDangNhap == tenDangNhap);

            if (khachHang == null)
            {
                return NotFound();
            }

            var donHangs = await _context.HoaDons
                .Where(d => d.MaKH == khachHang.MaKH)
                .Include(d => d.ChiTietHoaDons)
                .ThenInclude(ct => ct.DienThoai)
                .OrderByDescending(d => d.NgayLap)
                .ToListAsync();

            return View(donHangs);
        }

        // Tạo đơn hàng mới
        [Authorize(Policy = "CustomerOnly")]
        [HttpGet]
        public async Task<IActionResult> Create(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return BadRequest();
            }

            var product = await _context.DienThoais.FindAsync(productId);
            if (product == null || product.SoLuongTon <= 0)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại hoặc đã hết hàng.";
                return RedirectToAction("Index", "Shop");
            }

            var model = new CreateOrderViewModel
            {
                MaDT = product.MaDT,
                TenDT = product.TenDT,
                DonGia = product.DonGia,
                SoLuongTon = product.SoLuongTon,
                SoLuong = 1
            };

            return View(model);
        }

        // Xử lý tạo đơn hàng
        // Thay thế method Create
        [Authorize(Policy = "CustomerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var tenDangNhap = User.Identity.Name;
                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.TenDangNhap == tenDangNhap);

                var product = await _context.DienThoais.FindAsync(model.MaDT);

                if (khachHang == null || product == null)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại.";
                    return View(model);
                }

                if (product.SoLuongTon < model.SoLuong)
                {
                    ModelState.AddModelError("SoLuong", "Không đủ hàng trong kho.");
                    model.TenDT = product.TenDT;
                    model.DonGia = product.DonGia;
                    model.SoLuongTon = product.SoLuongTon;
                    return View(model);
                }

                // Tạo mã hóa đơn unique
                var maHD = await GenerateUniqueOrderId();

                // Tạo hóa đơn
                var hoaDon = new HoaDon
                {
                    MaHD = maHD,
                    MaKH = khachHang.MaKH,
                    NgayLap = DateTime.Now,
                    TongTien = model.SoLuong * product.DonGia
                };

                // Tạo chi tiết hóa đơn
                var chiTiet = new ChiTietHoaDon
                {
                    MaHD = maHD,
                    MaDT = model.MaDT,
                    SoLuong = model.SoLuong,
                    DonGia = product.DonGia
                };

                // Cập nhật tồn kho
                product.SoLuongTon -= model.SoLuong;

                _context.HoaDons.Add(hoaDon);
                _context.ChiTietHoaDons.Add(chiTiet);
                _context.Update(product);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng: {maHD}";
                return RedirectToAction("OrderSuccess", new { id = maHD });
            }

            // Reload thông tin sản phẩm nếu có lỗi
            var productInfo = await _context.DienThoais.FindAsync(model.MaDT);
            if (productInfo != null)
            {
                model.TenDT = productInfo.TenDT;
                model.DonGia = productInfo.DonGia;
                model.SoLuongTon = productInfo.SoLuongTon;
            }

            return View(model);
        }

        // Trang thành công
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> OrderSuccess(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(ct => ct.DienThoai)
                .Include(h => h.KhachHang)
                .FirstOrDefaultAsync(h => h.MaHD == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền truy cập
            var tenDangNhap = User.Identity.Name;
            if (hoaDon.KhachHang?.TenDangNhap != tenDangNhap)
            {
                return Forbid();
            }

            return View(hoaDon);
        }

        // Chi tiết đơn hàng
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(ct => ct.DienThoai)
                .Include(h => h.KhachHang)
                .Include(h => h.QuanTriVien)
                .FirstOrDefaultAsync(h => h.MaHD == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền truy cập
            if (User.HasClaim("VaiTro", "khach"))
            {
                var tenDangNhap = User.Identity.Name;
                if (hoaDon.KhachHang?.TenDangNhap != tenDangNhap)
                {
                    return Forbid();
                }
            }

            return View(hoaDon);
        }

        private async Task<string> GenerateUniqueOrderId()
        {
            string newId;
            bool exists;

            do
            {
                var lastOrder = await _context.HoaDons
                    .OrderByDescending(h => h.MaHD)
                    .FirstOrDefaultAsync();

                if (lastOrder == null)
                {
                    newId = "HD001";
                }
                else
                {
                    var lastNumber = int.Parse(lastOrder.MaHD.Substring(2));
                    newId = $"HD{(lastNumber + 1):D3}";
                }

                // Kiểm tra xem ID này đã tồn tại chưa
                exists = await _context.HoaDons.AnyAsync(h => h.MaHD == newId);

                // Nếu đã tồn tại, thêm timestamp để tránh trùng
                if (exists)
                {
                    var timestamp = DateTime.Now.ToString("mmss");
                    newId = $"HD{timestamp}";
                    exists = await _context.HoaDons.AnyAsync(h => h.MaHD == newId);
                }

            } while (exists);

            return newId;
        }
    }
}