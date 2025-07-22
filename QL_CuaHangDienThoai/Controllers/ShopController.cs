using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_CuaHangDienThoai.Data;

namespace QL_CuaHangDienThoai.Controllers
{
    [Authorize(Policy = "CustomerOnly")]
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            var dienThoais = from d in _context.DienThoais
                             where d.SoLuongTon > 0
                             select d;

            if (!String.IsNullOrEmpty(searchString))
            {
                dienThoais = dienThoais.Where(d => d.TenDT.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    dienThoais = dienThoais.OrderByDescending(d => d.TenDT);
                    break;
                case "price":
                    dienThoais = dienThoais.OrderBy(d => d.DonGia);
                    break;
                case "price_desc":
                    dienThoais = dienThoais.OrderByDescending(d => d.DonGia);
                    break;
                default:
                    dienThoais = dienThoais.OrderBy(d => d.TenDT);
                    break;
            }

            return View(await dienThoais.ToListAsync());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dienThoai = await _context.DienThoais
                .FirstOrDefaultAsync(m => m.MaDT == id);

            if (dienThoai == null || dienThoai.SoLuongTon <= 0)
            {
                return NotFound();
            }

            return View(dienThoai);
        }
    }
}