using AspNetCoreHero.ToastNotification.Abstractions;
using CHTC_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;

namespace CHTC_1.Controllers
{
    [Authorize]
    public class DonHangController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
        public DonHangController(Chtc8Context context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }
        public IActionResult Index(int page = 1)
        {
            var pageNumber = page;
            var pageSize = 4;
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                var khachhang = _context.NguoiDungs.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
                if (khachhang != null)
                {
                    var lsdonhang = _context.HoaDons
                        .Include(x => x.ChiTietHoaDons).Include(t => t.Tthd).Include(p =>p.Pttt)
                        .AsNoTracking()
                        .Where(x => x.NdId == khachhang.NdId)
                        .OrderByDescending(x => x.HdNgay)
                        .ToList();
                    //ViewBag.Donhang = lsdonhang;
                    PagedList<HoaDon> models = new PagedList<HoaDon>(lsdonhang.AsQueryable(), pageNumber, pageSize);
                    return View(models);
                }

            }
            return RedirectToAction("Accounts/Login");
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.HoaDons == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons
                .Include(h => h.Nd)
                .Include(h => h.Pttt)
                .Include(h => h.Tthd)
                .FirstOrDefaultAsync(m => m.HdId == id);
            if (hoaDon == null)
            {
                return NotFound();
            }
            var chitiethoadon = _context.ChiTietHoaDons
                .Include(s => s.Sp)
                .AsNoTracking()
                .Where(x => x.HdId == hoaDon.HdId)
                .OrderBy(x => x.SpId)
                .ToList();
            ViewBag.ChiTiet = chitiethoadon;
            return View(hoaDon);
        }

    }
}
