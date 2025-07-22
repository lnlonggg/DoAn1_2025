using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_CuaHangDienThoai.Data;
using QL_CuaHangDienThoai.Models;
using QL_CuaHangDienThoai.ViewModels;
using System.Security.Claims;

namespace QL_CuaHangDienThoai.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var taiKhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap && t.MatKhau == model.MatKhau);

                if (taiKhoan != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, taiKhoan.TenDangNhap),
                        new Claim(ClaimTypes.Email, taiKhoan.Email),
                        new Claim("VaiTro", taiKhoan.VaiTro)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(24)
                    };

                    await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Redirect theo vai trò
                    if (taiKhoan.VaiTro == "admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (taiKhoan.VaiTro == "nhanvien")
                    {
                        return RedirectToAction("Index", "DienThoai");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên đăng nhập đã tồn tại
                var existingAccount = await _context.TaiKhoans
                    .AnyAsync(t => t.TenDangNhap == model.TenDangNhap || t.Email == model.Email);

                if (existingAccount)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc email đã tồn tại.");
                    return View(model);
                }

                // Tạo tài khoản mới
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.TenDangNhap,
                    MatKhau = model.MatKhau,
                    Email = model.Email,
                    VaiTro = "khach"
                };

                _context.TaiKhoans.Add(taiKhoan);

                // Tạo thông tin khách hàng
                var khachHang = new KhachHang
                {
                    MaKH = GenerateKhachHangId(),
                    HoTen = model.HoTen,
                    SoDT = model.SoDT,
                    Email = model.Email,
                    TenDangNhap = model.TenDangNhap
                };

                _context.KhachHangs.Add(khachHang);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private string GenerateKhachHangId()
        {
            var lastKH = _context.KhachHangs
                .OrderByDescending(k => k.MaKH)
                .FirstOrDefault();

            if (lastKH == null)
            {
                return "KH001";
            }

            var lastNumber = int.Parse(lastKH.MaKH.Substring(2));
            return $"KH{(lastNumber + 1):D3}";
        }
    }
}