using CHTC_1.Areas.Admin.Models;
using CHTC_1.Models;
using CHTC_1.ModelViews;
using CHTC_1.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;

namespace CHTC_1.Controllers
{
    public class ShopController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        public ShopController(Chtc8Context context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        public async Task<IActionResult> Index(int page = 1, int LoaiID = 0)
        {
            var pageNumber = page;
            var pageSize = 12;
            List<SanPham> lsProducts = new List<SanPham>();
            if (LoaiID != 0)
            {
                lsProducts = _context.SanPhams.AsNoTracking().Where(x => x.LId == LoaiID).Include(s => s.LIdNavigation).Include(S => S.Ncc)
                .OrderByDescending(x => x.SpId).ToList();
            }
            else
            {
                lsProducts = _context.SanPhams.AsNoTracking().Include(s => s.LIdNavigation).Include(S => S.Ncc)
                .OrderByDescending(x => x.SpId).ToList();
            }

            PagedList<SanPham> models = new PagedList<SanPham>(lsProducts.AsQueryable(), pageNumber, pageSize);
            ViewData["Loai"] = new SelectList(_context.Loais, "LId", "LTenloai");
            ViewBag.CurrentLoaiID = LoaiID;
            ViewBag.CurrentPage = pageNumber;
            return View(models);
            //return _context.SanPhams != null ?
            //              View(await _context.SanPhams.ToListAsync()) :
            //              Problem("Entity set 'ChtcContext.SanPhams'  is null.");
        }

        public IActionResult Filtter(int LoaiID = 0)
        {
            var url = $"/Shop?LoaiID={LoaiID}";
            if (LoaiID == 0)
            {
                url = $"/Shop";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        public IActionResult FindProduct(string keyword)
        {
            List<SanPham> ls = new List<SanPham>();
            List<SanPham> ls2 = new List<SanPham>();
            ls2 = _context.SanPhams.AsNoTracking()
                                 .Include(a => a.LIdNavigation).Include(a => a.Ncc)
                                 .OrderByDescending(x => x.SpTensp)
                                 .ToList();
            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                return PartialView("FindProduct", ls2);
            }
            ls = _context.SanPhams.AsNoTracking()
                                  .Include(a => a.LIdNavigation).Include(a => a.Ncc)
                                  .Where(x => x.SpTensp.Contains(keyword))
                                  .OrderByDescending(x => x.SpTensp)
                                  .Take(5)
                                  .ToList();
            if (ls == null)
            {
                return PartialView("FindProduct", ls2);
            }
            else
            {
                return PartialView("FindProduct", ls);
            }
        }


        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var sanpham = await _context.SanPhams
                .Include(n => n.LIdNavigation).Include(a => a.Ncc)
                .FirstOrDefaultAsync(m => m.SpId == id);
                if (sanpham == null)
                {
                    return RedirectToAction("Index");
                }

                var lsSanpham = _context.SanPhams.AsNoTracking().Where(x => x.LId == sanpham.LId && x.SpId != id).Take(4).ToList();
                var taikhoanId = HttpContext.Session.GetString("CustomerId");
                var khachhang = _context.NguoiDungs.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanId));
                SanPhamViewModel sanPhamVM = new SanPhamViewModel();
                var lsComments = _context.DanhGia.Include(x => x.Nd).Include(x => x.Sp).Where(x => x.SpId == id).ToList();
                sanPhamVM.DanhGia = lsComments;
                sanPhamVM.SanPham = sanpham;
                if(khachhang != null)
                {
                    sanPhamVM.Avatar = khachhang.NdHinhanh;
                }
                ViewBag.Sanpham = lsSanpham;

                return View(sanPhamVM);
            }
            catch
            {
                return RedirectToAction("Index", "Shop");
            }
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(SanPhamViewModel sanPhamViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var taikhoanID = HttpContext.Session.GetString("CustomerId");
                    if (taikhoanID != null)
                    {
                        DanhGium danhGia = new DanhGium
                        {
                            DgNgay = DateTime.Now,
                            DgSao = sanPhamViewModel.Diem,
                            NdId = Int32.Parse(taikhoanID),
                            DgNoidung = sanPhamViewModel.NoiDung,
                            SpId = sanPhamViewModel.SpId

                        };
                        _context.Add(danhGia);
                        await _context.SaveChangesAsync();
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage","User", "Đã thích bình luận của bạn");
                    }
                }
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return Redirect("Details/" + sanPhamViewModel.SpId);
            }
            catch
            {
                return Redirect("Details/" + sanPhamViewModel.SpId);
            }

        }
        [HttpGet]
        public async Task<IActionResult> ShowListComment()
        {
            var dsComment = _context.DanhGia.ToList();
            var taikhoanId = HttpContext.Session.GetString("CustomerId");
            var khachhang = _context.NguoiDungs.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanId));
            var message = "Có người thích bình luận của bạn";

            await _hubContext.Clients.All.SendAsync("SendNotification", khachhang.NdHoten ,message);
            return Json(new { status = "success", dsComment });
        }
        [HttpPost]
        public async Task<JsonResult> DisLikeComment([FromBody]int dgId)
        {
            var comment = _context.DanhGia.SingleOrDefault(x => x.DgId == dgId);
            if(comment == null)
            {
                return Json(new { status = "error" });
            }
            else
            {
                if(comment.DgKhongthich == null)
                {
                    comment.DgKhongthich = 1;
                   
                }
                else
                {
                    comment.DgKhongthich += 1;

                }
                _context.Update(comment);
                await _context.SaveChangesAsync();
            }
            return Json(new { status = "success", comment });
        }
        [HttpPost]
        public async Task<JsonResult> UnDisLikeComment([FromBody] int dgId)
        {
            var comment = _context.DanhGia.SingleOrDefault(x => x.DgId == dgId);
            if (comment == null)
            {
                return Json(new { status = "error" });
            }
            else
            {
                
                    comment.DgKhongthich -= 1;

                _context.Update(comment);
                await _context.SaveChangesAsync();
            }
            return Json(new { status = "success", comment });
        }
        [HttpPost]
        public async Task<JsonResult> LikeComment([FromBody] int dgId)
        {
            var comment = _context.DanhGia.SingleOrDefault(x => x.DgId == dgId);
            if (comment == null)
            {
                return Json(new { status = "error" });
            }
            else
            {
                if (comment.DgThich == null)
                {
                    comment.DgThich = 1;

                }
                else
                {
                    comment.DgThich += 1;

                }
                _context.Update(comment);
                await _context.SaveChangesAsync();
            }
            return Json(new { status = "success", comment });
        }
        [HttpPost]
        public async Task<JsonResult> UnLikeComment([FromBody] int dgId)
        {
            var comment = _context.DanhGia.SingleOrDefault(x => x.DgId == dgId);
            if (comment == null)
            {
                return Json(new { status = "error" });
            }
            else
            {

                comment.DgThich -= 1;

                _context.Update(comment);
                await _context.SaveChangesAsync();
            }
            return Json(new { status = "success", comment });
        }
        //[HttpGet]
        //public async Task<IActionResult> ShowSubComment()
        //{
        //    var dsComment = _context.DanhGia.ToList();
        //    return Json(new { status = "success", dsComment });
        //}
        public async Task<IActionResult> CheckQuantity(int soluongmoi, int productid) {
            var product = _context.SanPhams.SingleOrDefault(s => s.SpId == productid);
                if(product.SpSoluongton < soluongmoi)
                {
                    return Json(new { status = "success", product });
                }
                else
                {
                    return Json(new { status = "error"});
                }

        }

    }
}
