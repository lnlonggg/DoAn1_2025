using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_CuaHangDienThoai.Data;
using QL_CuaHangDienThoai.Helpers;
using QL_CuaHangDienThoai.Models;
using QL_CuaHangDienThoai.ViewModels;

namespace QL_CuaHangDienThoai.Controllers
{
    [Authorize(Policy = "StaffOnly")]
    public class DienThoaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DienThoaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DienThoai
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var dienThoais = from d in _context.DienThoais select d;

            if (!String.IsNullOrEmpty(searchString))
            {
                dienThoais = dienThoais.Where(d => d.TenDT.Contains(searchString) || d.MaDT.Contains(searchString));
            }

            return View(await dienThoais.OrderBy(d => d.MaDT).ToListAsync());
        }

        // GET: DienThoai/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dienThoai = await _context.DienThoais
                .FirstOrDefaultAsync(m => m.MaDT == id);
            if (dienThoai == null)
            {
                return NotFound();
            }

            return View(dienThoai);
        }

        // GET: DienThoai/Create
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: DienThoai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra mã điện thoại đã tồn tại
                if (await _context.DienThoais.AnyAsync(d => d.MaDT == model.MaDT))
                {
                    ModelState.AddModelError("MaDT", "Mã điện thoại đã tồn tại.");
                    return View(model);
                }

                // Lưu file ảnh nếu có
                try
                {
                    if (model.HinhAnhFile != null)
                    {
                        await ImageHelper.SaveProductImage(model.HinhAnhFile, model.MaDT);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("HinhAnhFile", ex.Message);
                    return View(model);
                }

                var dienThoai = new DienThoai
                {
                    MaDT = model.MaDT,
                    TenDT = model.TenDT,
                    DonGia = model.DonGia,  // ← Đã sửa từ DonGia
                    SoLuongTon = model.SoLuongTon
                };

                _context.Add(dienThoai);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: DienThoai/Edit/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dienThoai = await _context.DienThoais.FindAsync(id);
            if (dienThoai == null)
            {
                return NotFound();
            }

            var model = new EditProductViewModel
            {
                MaDT = dienThoai.MaDT,
                TenDT = dienThoai.TenDT ?? "",
                HinhAnhHienTai = ImageHelper.GetProductImage(dienThoai.MaDT),
                DonGia = dienThoai.DonGia,  // ← Đã sửa từ DonGia
                SoLuongTon = dienThoai.SoLuongTon
            };

            return View(model);
        }

        // POST: DienThoai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(string id, EditProductViewModel model)
        {
            if (id != model.MaDT)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var dienThoai = await _context.DienThoais.FindAsync(id);
                    if (dienThoai == null)
                    {
                        return NotFound();
                    }

                    // Lưu file ảnh mới nếu có
                    if (model.HinhAnhFile != null)
                    {
                        await ImageHelper.SaveProductImage(model.HinhAnhFile, model.MaDT);
                    }

                    dienThoai.TenDT = model.TenDT;
                    dienThoai.DonGia = model.DonGia;
                    dienThoai.SoLuongTon = model.SoLuongTon;

                    _context.Update(dienThoai);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DienThoaiExists(model.MaDT))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("HinhAnhFile", ex.Message);
                    model.HinhAnhHienTai = ImageHelper.GetProductImage(model.MaDT);
                    return View(model);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: DienThoai/Delete/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dienThoai = await _context.DienThoais
                .FirstOrDefaultAsync(m => m.MaDT == id);
            if (dienThoai == null)
            {
                return NotFound();
            }

            return View(dienThoai);
        }

        // POST: DienThoai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var dienThoai = await _context.DienThoais.FindAsync(id);
            if (dienThoai != null)
            {
                // Kiểm tra xem có hóa đơn nào đang sử dụng sản phẩm này không
                var hasOrders = await _context.ChiTietHoaDons.AnyAsync(ct => ct.MaDT == id);
                if (hasOrders)
                {
                    TempData["ErrorMessage"] = "Không thể xóa sản phẩm vì đã có trong hóa đơn.";
                }
                else
                {
                    _context.DienThoais.Remove(dienThoai);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DienThoaiExists(string id)
        {
            return _context.DienThoais.Any(e => e.MaDT == id);
        }
    }
}