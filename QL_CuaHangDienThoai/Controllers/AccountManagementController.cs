using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_CuaHangDienThoai.Data;
using QL_CuaHangDienThoai.Models;
using QL_CuaHangDienThoai.ViewModels;

namespace QL_CuaHangDienThoai.Controllers
{
    [Authorize(Policy = "AdminOnly")] 
    public class AccountManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var accounts = await _context.TaiKhoans
                .Include(t => t.KhachHang)
                .Include(t => t.QuanTriVien)
                .OrderBy(t => t.VaiTro)
                .ThenBy(t => t.TenDangNhap)
                .ToListAsync();

            return View(accounts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountViewModel model)
        {
            Console.WriteLine($"=== CREATE ACCOUNT DEBUG ===");
            Console.WriteLine($"TenDangNhap: '{model.TenDangNhap}'");
            Console.WriteLine($"Email: '{model.Email}'");
            Console.WriteLine($"VaiTro: '{model.VaiTro}'");

            if (ModelState.IsValid)
            {
                if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Creating TaiKhoan with Email...");
                    
                    var taiKhoan = new TaiKhoan
                    {
                        TenDangNhap = model.TenDangNhap,
                        MatKhau = model.MatKhau,
                        Email = model.Email ?? string.Empty,
                        VaiTro = model.VaiTro
                    };
                    _context.TaiKhoans.Add(taiKhoan);
                    Console.WriteLine($"✅ TaiKhoan created with Email: {taiKhoan.Email}");

                   
                    if (model.VaiTro == "khach")
                    {
                        var maKH = await GenerateCustomerId();
                        var khachHang = new KhachHang
                        {
                            MaKH = maKH,
                            HoTen = model.HoTen ?? string.Empty,
                            SoDT = model.SoDienThoai ?? string.Empty,
                            Email = model.Email ?? string.Empty,
                            TenDangNhap = model.TenDangNhap
                        };
                        _context.KhachHangs.Add(khachHang);
                        Console.WriteLine($"✅ KhachHang created: {khachHang.MaKH}");
                    }
                    else
                    {
                        var maQTV = await GenerateStaffId();
                        var quanTriVien = new QuanTriVien
                        {
                            MaQTV = maQTV,
                            HoTen = model.HoTen ?? string.Empty,
                            SoDT = model.SoDienThoai ?? string.Empty,
                            Email = model.Email ?? string.Empty, 
                            TenDangNhap = model.TenDangNhap
                        };
                        _context.QuanTriViens.Add(quanTriVien);
                        Console.WriteLine($"✅ QuanTriVien created: {quanTriVien.MaQTV}");
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    Console.WriteLine("✅ All saved successfully with Email in TaiKhoan table");

                    TempData["SuccessMessage"] = $"Tạo tài khoản {model.TenDangNhap} thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ ERROR: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner: {ex.InnerException.Message}");
                    }
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
                }
            }

            return View(model);
        }

       
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.KhachHang)
                .Include(t => t.QuanTriVien)
                .FirstOrDefaultAsync(t => t.TenDangNhap == id);

            if (taiKhoan == null)
                return NotFound();

            var model = new EditAccountViewModel
            {
                TenDangNhap = taiKhoan.TenDangNhap,
                VaiTro = taiKhoan.VaiTro,
                HoTen = taiKhoan.VaiTro == "khach" ? taiKhoan.KhachHang?.HoTen : taiKhoan.QuanTriVien?.HoTen,
                SoDienThoai = taiKhoan.VaiTro == "khach" ? taiKhoan.KhachHang?.SoDT : taiKhoan.QuanTriVien?.SoDT,
                Email = taiKhoan.VaiTro == "khach" ? taiKhoan.KhachHang?.Email : taiKhoan.QuanTriVien?.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditAccountViewModel model)
        {
            if (id != model.TenDangNhap)
                return NotFound();

            if (model.TenDangNhap == User.Identity.Name)
            {
                ModelState.AddModelError("", "Không thể chỉnh sửa tài khoản hiện tại của bạn.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var taiKhoan = await _context.TaiKhoans
                    .Include(t => t.KhachHang)
                    .Include(t => t.QuanTriVien)
                    .FirstOrDefaultAsync(t => t.TenDangNhap == id);

                if (taiKhoan == null)
                    return NotFound();

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                   
                    if (!string.IsNullOrEmpty(model.MatKhauMoi))
                    {
                        taiKhoan.MatKhau = model.MatKhauMoi;
                    }

                    
                    taiKhoan.Email = model.Email ?? string.Empty;

                    if (taiKhoan.VaiTro == "khach" && taiKhoan.KhachHang != null)
                    {
                        taiKhoan.KhachHang.HoTen = model.HoTen ?? string.Empty;
                        taiKhoan.KhachHang.SoDT = model.SoDienThoai ?? string.Empty;
                        taiKhoan.KhachHang.Email = model.Email ?? string.Empty;
                        _context.Update(taiKhoan.KhachHang);
                    }
                    else if (taiKhoan.QuanTriVien != null)
                    {
                        taiKhoan.QuanTriVien.HoTen = model.HoTen ?? string.Empty;
                        taiKhoan.QuanTriVien.SoDT = model.SoDienThoai ?? string.Empty;
                        taiKhoan.QuanTriVien.Email = model.Email ?? string.Empty;
                        _context.Update(taiKhoan.QuanTriVien);
                    }

                    _context.Update(taiKhoan); 
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật tài khoản.");
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string username, string newRole)
        {
            if (username == User.Identity.Name)
            {
                return Json(new { success = false, message = "Không thể thay đổi vai trò tài khoản hiện tại của bạn." });
            }

            var taiKhoan = await _context.TaiKhoans.FindAsync(username);
            if (taiKhoan == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });
            }

            if (taiKhoan.VaiTro == newRole)
            {
                return Json(new { success = false, message = "Tài khoản đã có vai trò này." });
            }

            try
            {
                taiKhoan.VaiTro = newRole;
                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã thay đổi vai trò thành {newRole}." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thay đổi vai trò." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAccount(string username)
        {
            if (username == User.Identity.Name)
            {
                return Json(new { success = false, message = "Không thể khóa tài khoản hiện tại của bạn." });
            }

            var taiKhoan = await _context.TaiKhoans.FindAsync(username);
            if (taiKhoan == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });
            }

            return Json(new { success = true, message = "Tính năng khóa tài khoản sẽ được phát triển tiếp." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string username)
        {
            if (username == User.Identity.Name)
            {
                return Json(new { success = false, message = "Không thể xóa tài khoản hiện tại của bạn." });
            }

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.KhachHang)
                .Include(t => t.QuanTriVien)
                .FirstOrDefaultAsync(t => t.TenDangNhap == username);

            if (taiKhoan == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });
            }


            if (taiKhoan.VaiTro == "khach")
            {
                var hasOrders = await _context.HoaDons.AnyAsync(h => h.MaKH == taiKhoan.KhachHang.MaKH);
                if (hasOrders)
                {
                    return Json(new { success = false, message = "Không thể xóa khách hàng đã có đơn hàng." });
                }
            }
            else
            {
                var hasProcessedOrders = await _context.HoaDons.AnyAsync(h => h.MaQTV == taiKhoan.QuanTriVien!.MaQTV);
                if (hasProcessedOrders)
                {
                    return Json(new { success = false, message = "Không thể xóa nhân viên đã xử lý đơn hàng." });
                }
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                if (taiKhoan.KhachHang != null)
                    _context.KhachHangs.Remove(taiKhoan.KhachHang);

                if (taiKhoan.QuanTriVien != null)
                    _context.QuanTriViens.Remove(taiKhoan.QuanTriVien);

                _context.TaiKhoans.Remove(taiKhoan);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Xóa tài khoản thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa tài khoản." });
            }
        }

        private async Task<string> GenerateCustomerId()
        {
            string newId;
            bool exists;

            do
            {
                var lastCustomer = await _context.KhachHangs
                    .OrderByDescending(k => k.MaKH)
                    .FirstOrDefaultAsync();

                if (lastCustomer == null)
                {
                    newId = "KH001";
                }
                else
                {
                    var lastNumber = int.Parse(lastCustomer.MaKH.Substring(2));
                    newId = $"KH{(lastNumber + 1):D3}";
                }

                Console.WriteLine($"Generated potential MaKH: {newId}");

                exists = await _context.KhachHangs.AnyAsync(k => k.MaKH == newId);

                if (exists)
                {
                    Console.WriteLine($"MaKH {newId} already exists, generating new one...");
                    var timestamp = DateTime.Now.ToString("mmss");
                    newId = $"KH{timestamp}";
                    exists = await _context.KhachHangs.AnyAsync(k => k.MaKH == newId);
                }

            } while (exists);

            Console.WriteLine($"Final unique MaKH: {newId}");
            return newId;
        }

        private async Task<string> GenerateStaffId()
        {
            string newId;
            bool exists;

            do
            {
                var lastStaff = await _context.QuanTriViens
                    .OrderByDescending(q => q.MaQTV)
                    .FirstOrDefaultAsync();

                if (lastStaff == null)
                {
                    newId = "QTV001";
                }
                else
                {
    
                    var lastNumber = int.Parse(lastStaff.MaQTV.Substring(3));
                    newId = $"QTV{(lastNumber + 1):D3}";
                }

                Console.WriteLine($"Generated potential MaQTV: {newId}");


                exists = await _context.QuanTriViens.AnyAsync(q => q.MaQTV == newId);

                if (exists)
                {
                    Console.WriteLine($"MaQTV {newId} already exists, generating new one...");

                    var timestamp = DateTime.Now.ToString("mmss");
                    newId = $"QTV{timestamp}";
                    exists = await _context.QuanTriViens.AnyAsync(q => q.MaQTV == newId);
                }

            } while (exists);

            Console.WriteLine($"Final unique MaQTV: {newId}");
            return newId;
        }
    }
}