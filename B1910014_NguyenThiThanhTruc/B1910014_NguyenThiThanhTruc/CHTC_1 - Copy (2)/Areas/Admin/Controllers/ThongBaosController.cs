using AspNetCoreHero.ToastNotification.Abstractions;
using CHTC_1.Models;
using CHTC_1.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CHTC_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin,Staff")]
    public class ThongBaosController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        public ThongBaosController(Chtc8Context context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> CheckoutNotification([FromBody] int idNd)
        {
            var count = 0;
            var thongBao = new ThongBao();
            var nguoiDung = _context.NguoiDungs.SingleOrDefault(n => n.NdId == idNd);
            var taikhoanID = HttpContext.Session.GetString("Admin");
            var taikhoan = _context.TaiKhoans.SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
            //if (nguoiDung == null)
            //{
            //    return Json(new { status = "error" });
            //}
            //else
            //{
            //    thongBao.TbTieude = "Đơn đặt hàng mới";
            //    thongBao.TbNoidung = nguoiDung.NdHoten + " đã đặt một đơn hàng. Xác nhận ngay!";
            //    thongBao.TbNguoigui = nguoiDung.NdId;
            //    thongBao.TbThoigian = DateTime.Today;
            //    thongBao.TbTrangthai = 0;
            //    thongBao.TbNguoinhan = taikhoan.NdId;
            //}
            //_context.Add(thongBao);
            //await _context.SaveChangesAsync();
            var dsthongbao = await _context.ThongBaos.Where(t => t.TbNguoinhan == 0).OrderByDescending(t => t.TbThoigian).ToListAsync();
            for(int i = 0; i < dsthongbao.Count(); i++)
            {
                if (dsthongbao[i].TbTrangthai == 0)
                    count++;
            }
            return Json(new { status = "success",  dsthongbao, count });
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> GetNotificationCus([FromBody] int idNd)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if(idNd == Convert.ToInt32(taikhoanID))
            {
                var count = 0;
                var nguoidung = _context.NguoiDungs.SingleOrDefault(x => x.NdId == idNd);
                var dsthongbao = await _context.ThongBaos.Where(t => t.TbNguoinhan == idNd).OrderByDescending(t => t.TbThoigian).ToListAsync();
                for (int i = 0; i < dsthongbao.Count(); i++)
                {
                    if (dsthongbao[i].TbTrangthai == 0)
                        count++;
                }
                return Json(new { status = "success", dsthongbao, count, taikhoanID});
            }
            return Json(new { status = "error" });
        }
        //đếm thông báo khi đăng nhập vào
        public async Task<IActionResult> CountNotificationCus()
        {
            var count = 0;
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            var dsthongbao = await _context.ThongBaos.Where(t => t.TbNguoinhan == Convert.ToInt32(taikhoanID)).OrderByDescending(t => t.TbThoigian).ToListAsync();
            for (int i = 0; i < dsthongbao.Count(); i++)
            {
                if (dsthongbao[i].TbTrangthai == 0)
                    count++;
            }
            return Json(new { status = "success", dsthongbao, count });
        }

        public async Task<IActionResult> SetStatus()
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if(taikhoanID != null){
                var countnotificationCus = await _context.ThongBaos.Where(t => t.TbTrangthai == 0 && t.TbNguoinhan == Convert.ToInt32(taikhoanID)).OrderByDescending(x => x.TbId).ToListAsync();
                for (var i = 0; i < countnotificationCus.Count(); i++)
                {
                    countnotificationCus[i].TbTrangthai = 1;
                    _context.Update(countnotificationCus[i]);
                }
                var aftersetstatus = await _context.ThongBaos.Where(t => t.TbNguoinhan == Convert.ToInt32(taikhoanID)).OrderByDescending(x => x.TbId).ToListAsync();
                await _context.SaveChangesAsync();

                return Json(new { status = "success", countnotificationCus, aftersetstatus });
            }

            return Json(new { status = "error" });

        }

        public IActionResult Index()
        {
            return View();
        }



    }
}
