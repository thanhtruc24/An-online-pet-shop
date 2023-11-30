using CHTC_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CHTC_1.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class SearchController : Controller
    {
        private readonly Chtc8Context _context;

        public SearchController(Chtc8Context context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult FindProduct(string keyword)
        {
            List<SanPham> ls = new List<SanPham>();
            List<SanPham> ls2 = new List<SanPham>();
            ls2 = _context.SanPhams.AsNoTracking()
                                  .Include(a => a.LIdNavigation).Include(a => a.Ncc)
                                  .OrderByDescending(x => x.SpTensp)
                                  .Take(5)
                                  .ToList();
            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                return PartialView("ListProductsSearchPartial", ls2);
            }
            ls = _context.SanPhams.AsNoTracking()
                                  .Include(a => a.LIdNavigation).Include( a => a.Ncc)
                                  .Where(x => x.SpTensp.Contains(keyword))
                                  .OrderByDescending(x => x.SpTensp)
                                  .Take(5)
                                  .ToList();
            if (ls == null)
            {
                return PartialView("ListProductsSearchPartial",ls2);
            }
            else
            {
                return PartialView("ListProductsSearchPartial", ls);
            }
        }
    }
}
