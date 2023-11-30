using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CHTC_1.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using PagedList.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CHTC_1.Notification;

namespace CHTC_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class HoaDonsController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _environment;
		private readonly IHubContext<NotificationHub> _hubContext;
		public HoaDonsController(Chtc8Context context, INotyfService notyf, IWebHostEnvironment environment, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _notyf = notyf;
            _environment = environment;
			_hubContext = hubContext;
		}

        // GET: Admin/HoaDons
        public async Task<IActionResult> Index(int? page)
        {
            ViewData["TrangThai"] = new SelectList(_context.TrangThaiHoaDons, "TthdId", "TthdTrangthai");
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var lsReceipt = _context.HoaDons.AsNoTracking().Include(h => h.Nd).Include(h => h.Pttt).Include(h => h.Tthd)
                .OrderByDescending(x => x.HdId);
            PagedList<HoaDon> models = new PagedList<HoaDon>(lsReceipt, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
            //var chtcContext = _context.HoaDons.Include(h => h.Nd).Include(h => h.Pttt).Include(h => h.Tthd);
            //return View(await chtcContext.ToListAsync());
        }

        // GET: Admin/HoaDons/Details/5
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
            var Chitietdonhang = _context.ChiTietHoaDons
                .Include(x => x.Sp)
                .AsNoTracking()
                .Where(x => x.HdId == hoaDon.HdId)
                .OrderBy(x => x.SpId)
                .ToList();

            ViewBag.ChiTiet = Chitietdonhang;

            return View(hoaDon);
        }

        // GET: Admin/HoaDons/Create
        public IActionResult Create()
        {
            ViewData["ThanhToan"] = new SelectList(_context.PhuongThucThanhToans, "PtttId", "PtttTen");
            ViewData["TrangThai"] = new SelectList(_context.TrangThaiHoaDons, "TthdId", "TthdTrangthai");
            return View();
        }

        // POST: Admin/HoaDons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HdId,PtttId,NdId,TthdId,HdTongtien,HdNgay")] HoaDon hoaDon)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hoaDon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ThanhToan"] = new SelectList(_context.PhuongThucThanhToans, "PtttId", "PtttTen", hoaDon.PtttId);
            ViewData["TrangThai"] = new SelectList(_context.TrangThaiHoaDons, "TthdId", "TthdTrangthai", hoaDon.TthdId);
            return View(hoaDon);
        }

        // GET: Admin/HoaDons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.HoaDons == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons.FindAsync(id);
            if (hoaDon == null)
            {
                return NotFound();
            }
            ViewData["NguoiDung"] = new SelectList(_context.NguoiDungs, "NdId", "NdEmail");
            ViewData["ThanhToan"] = new SelectList(_context.PhuongThucThanhToans, "PtttId", "PtttTen");
            ViewData["TrangThai"] = new SelectList(_context.TrangThaiHoaDons, "TthdId", "TthdTrangthai");
            return View(hoaDon);
        }

        // POST: Admin/HoaDons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HdId,PtttId,NdId,TthdId,HdTongtien,HdNgay")] HoaDon hoaDon)
        {
            if (id != hoaDon.HdId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hoaDon);
					// thong bao realtime
					var taikhoanID = HttpContext.Session.GetString("Admin");
					var admin = _context.TaiKhoans.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
					var infoAdmin = _context.NguoiDungs.AsNoTracking().FirstOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
                    var statusReceipt = _context.TrangThaiHoaDons.Where(t => t.TthdId == hoaDon.TthdId).Select(t => t.TthdTrangthai).FirstOrDefault();
					var thongBao = new ThongBao();
					thongBao.TbTieude = "Trạng thái đơn hàng";
					thongBao.TbNoidung = "Đơn hàng của bạn đã được chuyển sang: " + statusReceipt;
					thongBao.TbNguoigui = admin.NdId;
					thongBao.TbThoigian = DateTime.Now;
					thongBao.TbTrangthai = 0;
                    thongBao.TbNguoinhan = hoaDon.NdId;
					thongBao.TbAvt = infoAdmin.NdHinhanh;
					_context.Add(thongBao);
					await _context.SaveChangesAsync();
					var message = "Đơn hàng của bạn được cập nhật: ";
					await _hubContext.Clients.All.SendAsync("SendNotification", hoaDon.NdId, message);
				}
                catch (DbUpdateConcurrencyException)
                {
                    if (!HoaDonExists(hoaDon.HdId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                _notyf.Success("Cập nhật hóa đơn thành công");
                return RedirectToAction(nameof(Index));
            }
            ViewData["NguoiDung"] = new SelectList(_context.NguoiDungs, "NdId", "NdEmail");
            ViewData["ThanhToan"] = new SelectList(_context.PhuongThucThanhToans, "PtttId", "PtttTen");
            ViewData["TrangThai"] = new SelectList(_context.TrangThaiHoaDons, "TthdId", "TthdTrangthai");
            return View(hoaDon);
        }

        // GET: Admin/HoaDons/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.HoaDons == null)
        //    {
        //        return NotFound();
        //    }

        //    var hoaDon = await _context.HoaDons
        //        .Include(h => h.Nd)
        //        .Include(h => h.Pttt)
        //        .Include(h => h.Tthd)
        //        .FirstOrDefaultAsync(m => m.HdId == id);
        //    if (hoaDon == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(hoaDon);
        //}

        //// POST: Admin/HoaDons/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.HoaDons == null)
        //    {
        //        return Problem("Entity set 'ChtcContext.HoaDons'  is null.");
        //    }
        //    var hoaDon = await _context.HoaDons.FindAsync(id);
        //    var chitiethoadon = _context.ChiTietHoaDons.ToList();
        //    if (hoaDon != null)
        //    {
        //        for(int i =0; i < chitiethoadon.Count(); i++)
        //        {
        //            if (chitiethoadon[i].HdId == id)
        //                _context.ChiTietHoaDons.Remove(chitiethoadon[i]);
        //        }
        //        _context.HoaDons.Remove(hoaDon);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpGet]
        public async Task<JsonResult> DeleteOne(int? id)
        {
            var hoadon = await _context.HoaDons.FirstOrDefaultAsync(m => m.HdId == id);
            var nguoidung = await _context.NguoiDungs.Where(n => n.NdId == hoadon.NdId).Select(n => n.NdHoten).FirstOrDefaultAsync();
            return Json(new { status = "success", hoadon, nguoidung });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOneConfirm([FromBody] int hoadonId)
        {
            if (_context.HoaDons == null)
            {
                return Problem("Entity set 'ChtcContext.NhaCungCaps'  is null.");
            }
            var hoadon = await _context.HoaDons.FindAsync(hoadonId);
            var chitiethoadon = _context.ChiTietHoaDons.ToList();
            if (hoadon != null)
            {
                for (int i = 0; i < chitiethoadon.Count(); i++)
                {
                    if (chitiethoadon[i].HdId == hoadonId)
                        _context.ChiTietHoaDons.Remove(chitiethoadon[i]);
                }
                _context.HoaDons.Remove(hoadon);
                await _context.SaveChangesAsync();
                return Json(new { status = "success" });

            }
            return Json(new { status = "error" });
        }


        [HttpPost]
        public async Task<JsonResult> DeleteMany([FromBody] List<int> hoadonIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var hoadons = await _context.HoaDons.Where(p => hoadonIds.Contains(p.HdId)).ToListAsync();

                return Json(new { status = "success", hoadons });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cóH
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteManyConfirm([FromBody] List<int> hoadonIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var chitiethoadon = _context.ChiTietHoaDons.ToList();
                var hoadons = await _context.HoaDons.Where(p => hoadonIds.Contains(p.HdId)).ToListAsync();
                for (var i = 0; i < hoadons.Count(); i++)
                {
                    for(var j = 0; j < chitiethoadon.Count(); j++)
                    {
                        if (hoadons[i].HdId == chitiethoadon[j].HdId)
                            _context.ChiTietHoaDons.Remove(chitiethoadon[j]);
                    }
                    _context.HoaDons.Remove(hoadons[i]);
                }
                await _context.SaveChangesAsync();

                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { status = "error", message = ex.Message });
            }
        }

        private bool HoaDonExists(int id)
        {
          return (_context.HoaDons?.Any(e => e.HdId == id)).GetValueOrDefault();
        }

     
    }
}
