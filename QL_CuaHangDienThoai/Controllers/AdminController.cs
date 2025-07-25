using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_CuaHangDienThoai.Data;
using QL_CuaHangDienThoai.Models;

namespace QL_CuaHangDienThoai.Controllers
{
    [Authorize(Policy = "StaffOnly")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TongSanPham = await _context.DienThoais.CountAsync();
            ViewBag.TongKhachHang = await _context.KhachHangs.CountAsync();
            ViewBag.TongHoaDon = await _context.HoaDons.CountAsync();
            ViewBag.TongDoanhThu = await _context.HoaDons.SumAsync(h => h.TongTien);

            var sanPhamBanChay = await (from ct in _context.ChiTietHoaDons
                                        join dt in _context.DienThoais on ct.MaDT equals dt.MaDT
                                        group ct by new { ct.MaDT, dt.TenDT } into g
                                        orderby g.Sum(x => x.SoLuong) descending
                                        select new
                                        {
                                            MaDT = g.Key.MaDT,
                                            TenDT = g.Key.TenDT,
                                            SoLuongBan = g.Sum(x => x.SoLuong),
                                            DoanhThu = g.Sum(x => x.SoLuong * x.DonGia)
                                        }).Take(5).ToListAsync();

            ViewBag.SanPhamBanChay = sanPhamBanChay;
            ViewBag.PendingPayments = await _context.ThanhToanTrucTuyens
                .CountAsync(p => p.TrangThai == TrangThaiThanhToan.ChoDuyet);

            return View();
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> QuanLyNguoiDung()
        {
            var users = await _context.TaiKhoans
                .Include(t => t.KhachHang)
                .Include(t => t.QuanTriVien)
                .OrderBy(t => t.VaiTro)
                .ThenBy(t => t.TenDangNhap)
                .ToListAsync();

            return View(users);
        }

        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> QuanLyHoaDon()
        {
            var orders = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.ChiTietHoaDons)
                .ThenInclude(ct => ct.DienThoai)
                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();

            return View(orders);
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> BaoCaoChiTiet(DateTime? fromDate, DateTime? toDate,
            string reportType = "overview", string category = "all", int page = 1, int pageSize = 10)
        {
            // Thiết lập ngày mặc định
            var defaultFromDate = DateTime.Today.AddDays(-30);
            var defaultToDate = DateTime.Today;

            var actualFromDate = fromDate ?? defaultFromDate;
            var actualToDate = toDate ?? defaultToDate;

            // Tạo báo cáo dựa trên loại được chọn
            var report = await GenerateReport(actualFromDate, actualToDate, reportType, category, page, pageSize);

            ViewBag.Report = report;
            ViewBag.FromDate = actualFromDate;
            ViewBag.ToDate = actualToDate;
            ViewBag.ReportType = reportType;
            ViewBag.Category = category;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            // Lấy danh sách categories có sẵn (dựa trên tên sản phẩm)
            ViewBag.Categories = await GetAvailableCategories();

            return View();
        }

        private async Task<object> GenerateReport(DateTime fromDate, DateTime toDate,
            string reportType, string category, int page, int pageSize)
        {
            var report = new
            {
                // Doanh thu theo ngày (7 ngày gần nhất hoặc trong khoảng thời gian)
                DoanhThuTheoNgay = await GetRevenueByDate(fromDate, toDate),

                // Top sản phẩm bán chạy với lọc category
                TopSanPham = await GetTopProducts(fromDate, toDate, category, page, pageSize),

                // Top khách hàng VIP
                TopKhachHang = await GetTopCustomers(fromDate, toDate, page, pageSize),

                // Thống kê tồn kho với phân trang
                ThongKeTonKho = await GetInventoryStatus(category, page, pageSize),

                // Tổng quan
                TongQuan = await GetOverviewStats(fromDate, toDate, category),

                // Báo cáo lợi nhuận (giả định lợi nhuận = 20% giá bán)
                LoiNhuan = await GetProfitReport(fromDate, toDate, category),

                // Thống kê theo tháng nếu cần
                ThongKeTheoThang = reportType == "monthly" ? await GetMonthlyStats(fromDate, toDate) : null,

                // Metadata cho phân trang
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalProducts = await GetTotalProductCount(category),
                    TotalPages = (int)Math.Ceiling((double)await GetTotalProductCount(category) / pageSize)
                }
            };

            return report;
        }

        private async Task<List<object>> GetRevenueByDate(DateTime fromDate, DateTime toDate)
        {
            // Nếu khoảng thời gian > 7 ngày, lấy theo khoảng thời gian, ngược lại lấy 7 ngày gần nhất
            var targetFromDate = (toDate - fromDate).Days > 7 ? fromDate : DateTime.Today.AddDays(-6);

            return await _context.HoaDons
                .Where(h => h.NgayLap >= targetFromDate && h.NgayLap <= toDate)
                .GroupBy(h => h.NgayLap.Date)
                .Select(g => new
                {
                    NgayDate = g.Key,
                    DoanhThu = g.Sum(h => h.TongTien),
                    SoDonHang = g.Count()
                })
                .OrderBy(x => x.NgayDate)
                .Cast<object>()
                .ToListAsync();
        }

        private async Task<List<object>> GetTopProducts(DateTime fromDate, DateTime toDate,
            string category, int page, int pageSize)
        {
            var query = from ct in _context.ChiTietHoaDons
                        join dt in _context.DienThoais on ct.MaDT equals dt.MaDT
                        join hd in _context.HoaDons on ct.MaHD equals hd.MaHD
                        where hd.NgayLap >= fromDate && hd.NgayLap <= toDate
                        select new { ct, dt, hd };

            // Lọc theo category (dựa trên tên sản phẩm chứa từ khóa)
            if (category != "all")
            {
                query = query.Where(x => x.dt.TenDT!.ToLower().Contains(category.ToLower()));
            }

            return await query
                .GroupBy(x => new { x.ct.MaDT, x.dt.TenDT, x.dt.DonGia })
                .Select(g => new
                {
                    MaDT = g.Key.MaDT,
                    TenDT = g.Key.TenDT,
                    DonGia = g.Key.DonGia,
                    SoLuongBan = g.Sum(x => x.ct.SoLuong),
                    DoanhThu = g.Sum(x => x.ct.SoLuong * x.ct.DonGia),
                    LoiNhuan = g.Sum(x => x.ct.SoLuong * x.ct.DonGia * 0.2m) // Giả định 20% lợi nhuận
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Cast<object>()
                .ToListAsync();
        }

        private async Task<List<object>> GetTopCustomers(DateTime fromDate, DateTime toDate, int page, int pageSize)
        {
            return await (from hd in _context.HoaDons
                          join kh in _context.KhachHangs on hd.MaKH equals kh.MaKH
                          where hd.NgayLap >= fromDate && hd.NgayLap <= toDate
                          group hd by new { kh.MaKH, kh.HoTen, kh.SoDT } into g
                          orderby g.Sum(h => h.TongTien) descending
                          select new
                          {
                              MaKH = g.Key.MaKH,
                              HoTen = g.Key.HoTen,
                              SoDT = g.Key.SoDT,
                              SoDonHang = g.Count(),
                              TongTien = g.Sum(h => h.TongTien)
                          })
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .Cast<object>()
                          .ToListAsync();
        }

        private async Task<List<object>> GetInventoryStatus(string category, int page, int pageSize)
        {
            var query = _context.DienThoais.AsQueryable();

            if (category != "all")
            {
                query = query.Where(dt => dt.TenDT!.ToLower().Contains(category.ToLower()));
            }

            return await query
                .Select(dt => new
                {
                    MaDT = dt.MaDT,
                    TenDT = dt.TenDT,
                    DonGia = dt.DonGia,
                    SoLuongTon = dt.SoLuongTon,
                    GiaTriTon = dt.SoLuongTon * dt.DonGia,
                    TrangThai = dt.SoLuongTon <= 5 ? "Sắp hết" : dt.SoLuongTon <= 10 ? "Ít" : "Đủ"
                })
                .OrderBy(dt => dt.SoLuongTon)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Cast<object>()
                .ToListAsync();
        }

        private async Task<object> GetOverviewStats(DateTime fromDate, DateTime toDate, string category)
        {
            var hoaDonQuery = _context.HoaDons.Where(h => h.NgayLap >= fromDate && h.NgayLap <= toDate);

            var inventoryQuery = _context.DienThoais.AsQueryable();
            if (category != "all")
            {
                inventoryQuery = inventoryQuery.Where(dt => dt.TenDT!.ToLower().Contains(category.ToLower()));
            }

            return new
            {
                TongDoanhThu30Ngay = await hoaDonQuery.SumAsync(h => (decimal?)h.TongTien) ?? 0,
                TongDonHang30Ngay = await hoaDonQuery.CountAsync(),
                DoanhThuTrungBinh = await hoaDonQuery.AverageAsync(h => (decimal?)h.TongTien) ?? 0,
                SanPhamSapHet = await inventoryQuery.CountAsync(dt => dt.SoLuongTon <= 5),
                TongGiaTriTonKho = await inventoryQuery.SumAsync(dt => dt.SoLuongTon * dt.DonGia),
                SanPhamTheoCategory = category != "all" ? await inventoryQuery.CountAsync() : 0
            };
        }

        private async Task<List<object>> GetProfitReport(DateTime fromDate, DateTime toDate, string category)
        {
            var query = from ct in _context.ChiTietHoaDons
                        join dt in _context.DienThoais on ct.MaDT equals dt.MaDT
                        join hd in _context.HoaDons on ct.MaHD equals hd.MaHD
                        where hd.NgayLap >= fromDate && hd.NgayLap <= toDate
                        select new { ct, dt, hd };

            if (category != "all")
            {
                query = query.Where(x => x.dt.TenDT!.ToLower().Contains(category.ToLower()));
            }

            return await query
                .GroupBy(x => new { x.ct.MaDT, x.dt.TenDT, x.dt.DonGia })
                .Select(g => new
                {
                    MaDT = g.Key.MaDT,
                    TenDT = g.Key.TenDT,
                    GiaBan = g.Key.DonGia,
                    GiaNhap = g.Key.DonGia * 0.8m, // Giả định giá nhập = 80% giá bán
                    SoLuongBan = g.Sum(x => x.ct.SoLuong),
                    DoanhThu = g.Sum(x => x.ct.SoLuong * x.ct.DonGia),
                    LoiNhuan = g.Sum(x => x.ct.SoLuong * x.ct.DonGia * 0.2m), // 20% lợi nhuận
                    TyLeLoiNhuan = 20.0 // 20%
                })
                .OrderByDescending(x => x.LoiNhuan)
                .Take(10)
                .Cast<object>()
                .ToListAsync();
        }

        private async Task<List<object>> GetMonthlyStats(DateTime fromDate, DateTime toDate)
        {
            return await _context.HoaDons
                .Where(h => h.NgayLap >= fromDate && h.NgayLap <= toDate)
                .GroupBy(h => new { h.NgayLap.Year, h.NgayLap.Month })
                .Select(g => new
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    DoanhThu = g.Sum(h => h.TongTien),
                    SoDonHang = g.Count(),
                    DoanhThuTrungBinh = g.Average(h => h.TongTien)
                })
                .OrderBy(x => x.Nam).ThenBy(x => x.Thang)
                .Cast<object>()
                .ToListAsync();
        }

        private async Task<List<string>> GetAvailableCategories()
        {
            var categories = new List<string> { "all" };

            // Tự động phát hiện categories dựa trên tên sản phẩm
            var productNames = await _context.DienThoais
                .Where(dt => dt.TenDT != null)
                .Select(dt => dt.TenDT!.ToLower())
                .ToListAsync();

            if (productNames.Any(name => name.Contains("iphone") || name.Contains("ios")))
                categories.Add("iphone");

            if (productNames.Any(name => name.Contains("samsung") || name.Contains("android")))
                categories.Add("samsung");

            if (productNames.Any(name => name.Contains("xiaomi")))
                categories.Add("xiaomi");

            if (productNames.Any(name => name.Contains("oppo")))
                categories.Add("oppo");

            return categories;
        }

        private async Task<int> GetTotalProductCount(string category)
        {
            var query = _context.DienThoais.AsQueryable();
            if (category != "all")
            {
                query = query.Where(dt => dt.TenDT!.ToLower().Contains(category.ToLower()));
            }
            return await query.CountAsync();
        }

        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> XacNhanThanhToan()
        {
            var pendingPayments = await _context.ThanhToanTrucTuyens
                .Include(p => p.HoaDon)
                .ThenInclude(h => h.KhachHang)
                .Include(p => p.HoaDon.ChiTietHoaDons)
                .ThenInclude(ct => ct.DienThoai)
                .Where(p => p.TrangThai == TrangThaiThanhToan.ChoDuyet)
                .OrderByDescending(p => p.NgayTao)
                .ToListAsync();

            return View(pendingPayments);
        }

        [HttpPost]
        public async Task<IActionResult> ApprovePayment(string paymentId)
        {
            var payment = await _context.ThanhToanTrucTuyens
                .Include(p => p.HoaDon)
                .FirstOrDefaultAsync(p => p.MaThanhToan == paymentId);

            if (payment != null)
            {
                var tenDangNhap = User.Identity.Name;
                var nhanVien = await _context.QuanTriViens
                    .FirstOrDefaultAsync(q => q.TenDangNhap == tenDangNhap);

                payment.TrangThai = TrangThaiThanhToan.DaThanhToan;
                payment.NgayCapNhat = DateTime.Now;
                payment.ThongTinThem += $" - Được xác nhận bởi {User.Identity.Name} lúc {DateTime.Now:dd/MM/yyyy HH:mm}";

                if (payment.HoaDon != null && nhanVien != null)
                {
                    payment.HoaDon.MaQTV = nhanVien.MaQTV;
                    _context.Update(payment.HoaDon);
                }

                _context.Update(payment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã xác nhận thanh toán thành công!";
            }

            return RedirectToAction("XacNhanThanhToan");
        }

        [HttpPost]
        public async Task<IActionResult> RejectPayment(string paymentId, string reason)
        {
            var payment = await _context.ThanhToanTrucTuyens
                .FirstOrDefaultAsync(p => p.MaThanhToan == paymentId);

            if (payment != null)
            {
                payment.TrangThai = TrangThaiThanhToan.ThatBai;
                payment.NgayCapNhat = DateTime.Now;
                payment.ThongTinThem += $" - Bị từ chối bởi {User.Identity.Name} lúc {DateTime.Now:dd/MM/yyyy HH:mm}. Lý do: {reason}";

                _context.Update(payment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã từ chối thanh toán!";
            }

            return RedirectToAction("XacNhanThanhToan");
        }

        // API endpoints cho AJAX
        [HttpGet]
        public async Task<JsonResult> GetReportData(DateTime fromDate, DateTime toDate,
            string reportType, string category, int page = 1)
        {
            try
            {
                var report = await GenerateReport(fromDate, toDate, reportType, category, page, 10);
                return Json(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}