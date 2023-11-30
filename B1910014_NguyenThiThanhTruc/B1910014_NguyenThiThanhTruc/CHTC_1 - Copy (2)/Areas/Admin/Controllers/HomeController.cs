using CHTC_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;

namespace CHTC_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class HomeController : Controller
    {
        private readonly Chtc8Context _context;
        public HomeController(Chtc8Context context)
        {   
            _context = context;
        }
        public IActionResult Index()
        {
            var product = _context.SanPhams.GroupBy(x => x.SpId).Count();
            var dondat = _context.HoaDons.Where(x => x.HdNgay.Date == DateTime.Today).GroupBy(x => x.HdId).Count();
           
            ViewBag.thongkesp = product;
            ViewBag.dondat = dondat;
            return View();
        }

        public async Task<IActionResult> GetTopSellingProducts()
        {
            var topproduct = _context.ChiTietHoaDons.AsNoTracking().Include(s => s.Sp)
                .GroupBy(s => s.Sp.SpTensp).Select(g => new
                {
                    Spten = g.Key,
                    Soluongban = g.Count()
                }).OrderByDescending(x => x.Soluongban).ToList();
            return Json(topproduct);
        }

        public async Task<IActionResult> GetTopSupplier()
        {
            var topsupplier = _context.ChiTietHoaDons.AsNoTracking().Include(s => s.Sp)
                .Join(_context.SanPhams, cthd => cthd.Sp.SpId, sp => sp.SpId, (cthd, sp) => new { cthd, sp })
                .Join(_context.NhaCungCaps, cthdsp => cthdsp.sp.NccId, ncc => ncc.NccId, (cthdsp, ncc) => new { cthdsp.cthd, cthdsp.sp, ncc })
                .GroupBy(s => s.ncc.NccTen).Select(g => new
                {
                    ncc = g.Key,
                    Soluongban = g.Count()
                }).OrderByDescending(x => x.Soluongban).ToList();
            return Json(topsupplier);
        }

        [HttpPost]
        public async Task<IActionResult> ShowOrder(int time)
        {
            
            if(time == 1)
            {       
                DateTime currentDate = DateTime.Today;
                DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;
                DateTime startOfWeek = currentDate.AddDays(-(int)currentDayOfWeek);
                DateTime endOfWeek = startOfWeek.AddDays(6);
                var orderlist = _context.HoaDons.AsNoTracking().Where(hd => hd.HdNgay >= startOfWeek && hd.HdNgay <= endOfWeek).ToList();
                var countByDayOfWeek = new Dictionary<DayOfWeek, int>
                {
                    { DayOfWeek.Sunday, 0 },
                    { DayOfWeek.Monday, 0 },
                    { DayOfWeek.Tuesday, 0 },
                    { DayOfWeek.Wednesday, 0 },
                    { DayOfWeek.Thursday, 0 },
                    { DayOfWeek.Friday, 0 },
                    { DayOfWeek.Saturday, 0 }
                };

                for (int i = 0; i < orderlist.Count(); i++)
                {
                    var orderinday = orderlist[i].HdNgay.DayOfWeek;
                    countByDayOfWeek[orderinday] ++;
                }
                return Json(countByDayOfWeek);
            }
            else if( time == 2)
            {
                var orderlist = _context.HoaDons.AsNoTracking().ToList();
                var countbyMonth = new Dictionary<int, int>
                {
                    {1, 0 },
                    {2, 0 },
                    {3, 0 },
                    {4, 0 },
                    {5, 0 },
                    {6, 0 },
                    {7, 0 },
                    {8, 0 },
                    {9, 0 },
                    {10, 0 },
                    {11, 0 },
                    {12, 0 }
                };
                for(int i = 0; i < orderlist.Count(); i++)
                {
                    var orderinmonth = orderlist[i].HdNgay.Month;
                    countbyMonth[orderinmonth]++;
                }
                return Json(countbyMonth);
            }
            else
            {
                var orderlist = _context.HoaDons.AsNoTracking().ToList();
                var countbyQuarter = new Dictionary<int, int>
                {
                    {1, 0 },
                    {2, 0 },
                    {3, 0 },
                    {4, 0 }
                };
                for(int i = 0; i < orderlist.Count();i++)
                {
                    var orderinquarter = (orderlist[i].HdNgay.Month - 1)/3 + 1;
                    countbyQuarter[orderinquarter]++;
                }
                return Json(countbyQuarter);
            }
            return Json(new { status = "error"});

        }

        [HttpGet]
        public async Task<IActionResult> Report(int? page,int time = 0)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 10;
            List<HoaDon> lsReceipt = new List<HoaDon>();
            if (time == 0)
            {
                DateTime currentDate = DateTime.Today;

                var query = _context.HoaDons.AsNoTracking().Where(hd => hd.HdNgay.Date == DateTime.Today && hd.TthdId == 10).Include(h => h.Nd).Include(h => h.Pttt).Include(h => h.Tthd)
              .OrderByDescending(x => x.HdId);
                lsReceipt = await query.ToListAsync();
            }
            else if (time == 1)
            {
                DateTime currentDate = DateTime.Today;
                DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;
                DateTime startOfWeek = currentDate.AddDays(-(int)currentDayOfWeek);
                DateTime endOfWeek = startOfWeek.AddDays(6);
                var query = _context.HoaDons.AsNoTracking().Where(hd => hd.HdNgay >= startOfWeek && hd.HdNgay <= endOfWeek && hd.TthdId == 10).Include(h => h.Nd).Include(h => h.Pttt).Include(h => h.Tthd)
               .OrderByDescending(x => x.HdId);
                lsReceipt = await query.ToListAsync();
            }
            //else
            //{
            //    var query = _context.HoaDons.AsNoTracking().Where(hd => hd.HdNgay >= startOfWeek && hd.HdNgay <= endOfWeek && hd.TthdId == 10).Include(h => h.Nd).Include(h => h.Pttt).Include(h => h.Tthd)
            //   .OrderByDescending(x => x.HdId);
            //    lsReceipt = await query.ToListAsync();
            //}
            decimal total = 0;
            for (var i = 0; i < lsReceipt.Count(); i++)
            {
                total += lsReceipt[i].HdTongtien;
            }
            ViewBag.Total = total;
            PagedList<HoaDon> models = new PagedList<HoaDon>(lsReceipt.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.time = time;
            return View(models);

        }
        [AllowAnonymous]
        public IActionResult Filter(int time = 0)
        {
            var url = $"/Admin/Home/Report?time={time}";
            if (time == 0)
            {
                url = $"/Admin/Home/Report";
            }
            return Json(new { status = "success", redirectUrl = url });
        }



        public async Task<IActionResult> SalesReport()
        {
            DateTime currentDate = DateTime.Today;
            DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;
            DateTime startOfWeek = currentDate.AddDays(-(int)currentDayOfWeek);
            DateTime endOfWeek = startOfWeek.AddDays(6);
            var orderlist = _context.HoaDons.AsNoTracking().Where(hd => hd.HdNgay >= startOfWeek && hd.HdNgay <= endOfWeek).ToList();
            return Json(new { status = "success", orderlist });
        }

        public async Task<IActionResult> CountNotification()
        {
            var count = 0;
            var countnotification = await _context.ThongBaos.Where(t => t.TbNguoinhan == 0).OrderByDescending(x => x.TbThoigian).ToListAsync();
            for(var i =0; i < countnotification.Count(); i++)
            {
                if (countnotification[i].TbTrangthai == 0)
                    count++;
            }
            return Json(new { status = "success", count });
        }

        public async Task<IActionResult> SetStatus()
        {
            var countnotification = await _context.ThongBaos.Where(t => t.TbTrangthai == 0 && t.TbNguoinhan == 0).OrderByDescending(x => x.TbId).ToListAsync();
            for (var i = 0; i < countnotification.Count(); i++)
            {
                countnotification[i].TbTrangthai = 1;
                _context.Update(countnotification[i]);
            }
            var aftersetstatus = await _context.ThongBaos.Where(t => t.TbNguoinhan == 0).OrderByDescending(x => x.TbId).ToListAsync();

            await _context.SaveChangesAsync();

            return Json(new { status = "success", countnotification, aftersetstatus });
        }
    }
}
