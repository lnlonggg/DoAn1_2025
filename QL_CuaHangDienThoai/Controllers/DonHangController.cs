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


                var maHD = await GenerateUniqueOrderId();

                var hoaDon = new HoaDon
                {
                    MaHD = maHD,
                    MaKH = khachHang.MaKH,
                    MaQTV = null, 
                    NgayLap = DateTime.Now,
                    TongTien = model.SoLuong * product.DonGia
                };


                var chiTiet = new ChiTietHoaDon
                {
                    MaHD = maHD,
                    MaDT = model.MaDT,
                    SoLuong = model.SoLuong,
                    DonGia = product.DonGia
                };

                product.SoLuongTon -= model.SoLuong;

                _context.HoaDons.Add(hoaDon);
                _context.ChiTietHoaDons.Add(chiTiet);
                _context.Update(product);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng: {maHD}";
                return RedirectToAction("OrderSuccess", new { id = maHD });
            }

            var productInfo = await _context.DienThoais.FindAsync(model.MaDT);
            if (productInfo != null)
            {
                model.TenDT = productInfo.TenDT;
                model.DonGia = productInfo.DonGia;
                model.SoLuongTon = productInfo.SoLuongTon;
            }

            return View(model);
        }


        [Authorize(Policy = "StaffOnly")]
        [HttpGet]
        public async Task<IActionResult> CreateForCustomer()
        {
            ViewBag.KhachHangs = await _context.KhachHangs.ToListAsync();
            ViewBag.SanPhams = await _context.DienThoais.Where(dt => dt.SoLuongTon > 0).ToListAsync();

            return View();
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForCustomer(string maKH, string maDT, int soLuong, string? ghiChu)
        {
            if (string.IsNullOrEmpty(maKH) || string.IsNullOrEmpty(maDT) || soLuong <= 0)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin.");
                ViewBag.KhachHangs = await _context.KhachHangs.ToListAsync();
                ViewBag.SanPhams = await _context.DienThoais.Where(dt => dt.SoLuongTon > 0).ToListAsync();
                return View();
            }

            var khachHang = await _context.KhachHangs.FindAsync(maKH);
            var product = await _context.DienThoais.FindAsync(maDT);

            if (khachHang == null || product == null)
            {
                TempData["ErrorMessage"] = "Thông tin khách hàng hoặc sản phẩm không hợp lệ.";
                return RedirectToAction("CreateForCustomer");
            }

            if (product.SoLuongTon < soLuong)
            {
                TempData["ErrorMessage"] = $"Sản phẩm {product.TenDT} chỉ còn {product.SoLuongTon} trong kho.";
                return RedirectToAction("CreateForCustomer");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tenDangNhap = User.Identity.Name;
                var nhanVien = await _context.QuanTriViens
                    .FirstOrDefaultAsync(q => q.TenDangNhap == tenDangNhap);

                var maHD = await GenerateUniqueOrderId();

                var hoaDon = new HoaDon
                {
                    MaHD = maHD,
                    MaKH = maKH,
                    MaQTV = nhanVien?.MaQTV,
                    NgayLap = DateTime.Now,
                    TongTien = soLuong * product.DonGia
                };


                var chiTiet = new ChiTietHoaDon
                {
                    MaHD = maHD,
                    MaDT = maDT,
                    SoLuong = soLuong,
                    DonGia = product.DonGia
                };

                product.SoLuongTon -= soLuong;

                _context.HoaDons.Add(hoaDon);
                _context.ChiTietHoaDons.Add(chiTiet);
                _context.Update(product);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Tạo đơn hàng {maHD} cho khách hàng {khachHang.HoTen} thành công!";
                return RedirectToAction("Details", new { id = maHD });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo đơn hàng. Vui lòng thử lại.";
                return RedirectToAction("CreateForCustomer");
            }
        }


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

            var tenDangNhap = User.Identity.Name;
            if (hoaDon.KhachHang?.TenDangNhap != tenDangNhap)
            {
                return Forbid();
            }

            return View(hoaDon);
        }

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

 
                exists = await _context.HoaDons.AnyAsync(h => h.MaHD == newId);

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