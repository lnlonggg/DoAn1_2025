using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_CuaHangDienThoai.Data;
using QL_CuaHangDienThoai.Models;
using System.Diagnostics;

namespace QL_CuaHangDienThoai.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var vaiTro = User.Claims.FirstOrDefault(c => c.Type == "VaiTro")?.Value;

                switch (vaiTro)
                {
                    case "admin":
                    case "nhanvien":
                        return RedirectToAction("Index", "Admin");
                    default:
                        break;
                }
            }

            var featuredProducts = await _context.DienThoais
                .Where(d => d.SoLuongTon > 0)
                .OrderByDescending(d => d.MaDT)
                .Take(6)
                .ToListAsync();

            ViewBag.TongSanPham = await _context.DienThoais.CountAsync();
            ViewBag.TongKhachHang = await _context.KhachHangs.CountAsync();

            return View(featuredProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}