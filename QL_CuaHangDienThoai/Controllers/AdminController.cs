using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_CuaHangDienThoai.Data;

namespace QL_CuaHangDienThoai.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Thống kê cơ bản
            ViewBag.TongSanPham = await _context.DienThoais.CountAsync();
            ViewBag.TongKhachHang = await _context.KhachHangs.CountAsync();
            ViewBag.TongHoaDon = await _context.HoaDons.CountAsync();
            ViewBag.TongDoanhThu = await _context.HoaDons.SumAsync(h => h.TongTien);

            // Sản phẩm bán chạy - sửa lỗi ThanhTien
            var sanPhamBanChay = await (from ct in _context.ChiTietHoaDons
                                        join dt in _context.DienThoais on ct.MaDT equals dt.MaDT
                                        group ct by new { ct.MaDT, dt.TenDT } into g
                                        orderby g.Sum(x => x.SoLuong) descending
                                        select new
                                        {
                                            MaDT = g.Key.MaDT,
                                            TenDT = g.Key.TenDT,
                                            SoLuongBan = g.Sum(x => x.SoLuong),
                                            DoanhThu = g.Sum(x => x.SoLuong * x.DonGia) // Sử dụng SoLuong * DonGia thay vì ThanhTien
                                        }).Take(5).ToListAsync();

            ViewBag.SanPhamBanChay = sanPhamBanChay;

            return View();
        }

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

        public async Task<IActionResult> BaoCaoChiTiet()
        {
            var fromDate = DateTime.Today.AddDays(-30); // 30 ngày trước
            var toDate = DateTime.Today;

            // Báo cáo tổng quan
            var report = new
            {
                // Doanh thu theo ngày (7 ngày gần nhất) - Sửa lỗi ToString
                DoanhThuTheoNgay = await _context.HoaDons
                    .Where(h => h.NgayLap >= DateTime.Today.AddDays(-6))
                    .GroupBy(h => h.NgayLap.Date)
                    .Select(g => new
                    {
                        NgayDate = g.Key,
                        DoanhThu = g.Sum(h => h.TongTien),
                        SoDonHang = g.Count()
                    })
                    .OrderBy(x => x.NgayDate)
                    .ToListAsync(),

                // Top sản phẩm bán chạy
                TopSanPham = await (from ct in _context.ChiTietHoaDons
                                    join dt in _context.DienThoais on ct.MaDT equals dt.MaDT
                                    join hd in _context.HoaDons on ct.MaHD equals hd.MaHD
                                    where hd.NgayLap >= fromDate
                                    group ct by new { ct.MaDT, dt.TenDT, dt.DonGia } into g
                                    orderby g.Sum(x => x.SoLuong) descending
                                    select new
                                    {
                                        MaDT = g.Key.MaDT,
                                        TenDT = g.Key.TenDT,
                                        DonGia = g.Key.DonGia,
                                        SoLuongBan = g.Sum(x => x.SoLuong),
                                        DoanhThu = g.Sum(x => x.SoLuong * x.DonGia)
                                    }).Take(10).ToListAsync(),

                // Top khách hàng
                TopKhachHang = await (from hd in _context.HoaDons
                                      join kh in _context.KhachHangs on hd.MaKH equals kh.MaKH
                                      where hd.NgayLap >= fromDate
                                      group hd by new { kh.MaKH, kh.HoTen, kh.SoDT } into g
                                      orderby g.Sum(h => h.TongTien) descending
                                      select new
                                      {
                                          MaKH = g.Key.MaKH,
                                          HoTen = g.Key.HoTen,
                                          SoDT = g.Key.SoDT,
                                          SoDonHang = g.Count(),
                                          TongTien = g.Sum(h => h.TongTien)
                                      }).Take(10).ToListAsync(),

                // Thống kê tồn kho
                ThongKeTonKho = await _context.DienThoais
                    .Select(dt => new
                    {
                        MaDT = dt.MaDT,
                        TenDT = dt.TenDT,
                        DonGia = dt.DonGia,
                        SoLuongTon = dt.SoLuongTon,
                        GiaTriTon = dt.SoLuongTon * dt.DonGia
                    })
                    .OrderBy(dt => dt.SoLuongTon)
                    .ToListAsync(),

                // Thống kê tổng quan
                TongQuan = new
                {
                    TongDoanhThu30Ngay = await _context.HoaDons
                        .Where(h => h.NgayLap >= fromDate)
                        .SumAsync(h => (decimal?)h.TongTien) ?? 0,

                    TongDonHang30Ngay = await _context.HoaDons
                        .CountAsync(h => h.NgayLap >= fromDate),

                    DoanhThuTrungBinh = await _context.HoaDons
                        .Where(h => h.NgayLap >= fromDate)
                        .AverageAsync(h => (decimal?)h.TongTien) ?? 0,

                    SanPhamSapHet = await _context.DienThoais
                        .CountAsync(dt => dt.SoLuongTon <= 5),

                    TongGiaTriTonKho = await _context.DienThoais
                        .SumAsync(dt => dt.SoLuongTon * dt.DonGia)
                }
            };

            ViewBag.Report = report;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View();
        }
    }
}