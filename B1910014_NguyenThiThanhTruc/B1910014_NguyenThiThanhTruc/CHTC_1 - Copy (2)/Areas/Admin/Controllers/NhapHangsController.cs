using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;
using CHTC_1.Areas.Admin.Models;
using CHTC_1.Extension;
using CHTC_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;

namespace CHTC_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class NhapHangsController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _environment;

        public NhapHangsController(Chtc8Context context, INotyfService notyf, IWebHostEnvironment environment)
        {
            _context = context;
            _notyf = notyf;
            _environment = environment;
        }
        public List<ChitietPhieuNhap> chitietPhieunhaphangs
        {
            get
            {
                var ct = HttpContext.Session.Get<List<ChitietPhieuNhap>>("chitietPhieunhaphangs");
                if (ct == default(List<ChitietPhieuNhap>))
                {
                    ct = new List<ChitietPhieuNhap>();
                }
                return ct;
            }
        }


        public async Task<IActionResult> Index(int? page)
        {
            //ViewData["TrangThai"] = new SelectList(_context.TrangThaiHoaDons, "TthdId", "TthdTrangthai");
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var lshoadonnhap = _context.PhieuNhapHangs.Include(n => n.Nd).AsNoTracking()
                .OrderByDescending(x => x.PnhId);
            PagedList<PhieuNhapHang> models = new PagedList<PhieuNhapHang>(lshoadonnhap, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
            //var chtcContext = _context.HoaDons.Include(h => h.Nd).Include(h => h.Pttt).Include(h => h.Tthd);
            //return View(await chtcContext.ToListAsync());
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PhieuNhapHangs == null)
            {
                return NotFound();
            }

            var phieunhap = await _context.PhieuNhapHangs
                .Include(h => h.Nd)
                .FirstOrDefaultAsync(m => m.PnhId == id);
            if (phieunhap == null)
            {
                return NotFound();
            }
            var chitietphieunhap = _context.ChitietPhieuNhaps
                .Include(x => x.Sp)
                .AsNoTracking()
                .Where(x => x.PnhId == phieunhap.PnhId)
                .OrderBy(x => x.SpId)
                .ToList();

            ViewBag.ChiTiet = chitietphieunhap;

            return View(phieunhap);
        }
        [HttpPost]
        public async Task<IActionResult> Nhaphang()
        {
            ViewData["Loai"] = new SelectList(_context.Loais, "LId", "LTenloai");
            ViewData["NhaCungCap"] = new SelectList(_context.NhaCungCaps, "NccId", "NccTen");
            ViewData["SanPham"] = new SelectList(_context.SanPhams, "SpId", "SpTensp");
            return View();
        }

        [AllowAnonymous]
        public IActionResult Filtter(int LoaiID = 0)
        {
            var url = $"/Admin/NhapHangs/Create?LoaiID={LoaiID}";
            if (LoaiID == 0)
            {
                url = $"/Admin/NhapHangs/Create";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        [AllowAnonymous]
        public IActionResult Filter(int nccID = 0)
        {
            var url = $"/Admin/NhapHangs/Create?nccID={nccID}";
            if (nccID == 0)
            {
                url = $"/Admin/NhapHangs/Create";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? nccId, int page = 1, int LoaiID = 0, int nccID = 0)
        {
            ViewData["Loai"] = new SelectList(_context.Loais, "LId", "LTenloai");
            ViewData["Ncc"] = new SelectList(_context.NhaCungCaps, "NccId", "NccTen");
            var staffId = HttpContext.Session.GetString("Admin");
            if (staffId == null)
            {
                return RedirectToAction("Login", "AdminAccount");
            }
            try
            {
                var pageNumber = page;
                var pageSize = 5;
                NhapHangViewModel phieunhaphang = new NhapHangViewModel();
                //phieunhaphang.nhaCungCaps = _context.NhaCungCaps.OrderBy(x => x.NccId).ToList();
                //if (nccId != null)
                //{
                //    phieuNhapViewModel.NccId = (int)nccId;
                //}
                if(LoaiID != 0)
                {
                    if(nccID != 0)
                    {
                        phieunhaphang.sanPhams = new PagedList<SanPham>(_context.SanPhams.Where(sp => sp.LId == LoaiID && sp.NccId == nccID).Include(n => n.Ncc).Include(n => n.LIdNavigation).OrderBy(x => x.SpId), pageNumber, pageSize);

                    }
                    else
                    {
                        phieunhaphang.sanPhams = new PagedList<SanPham>(_context.SanPhams.Where(sp => sp.LId == LoaiID).Include(n => n.Ncc).Include(n => n.LIdNavigation).OrderBy(x => x.SpId), pageNumber, pageSize);
                    }

                }
                else
                {
                    if(nccID != 0)
                    {
                        phieunhaphang.sanPhams = new PagedList<SanPham>(_context.SanPhams.Where(sp => sp.NccId == nccID).Include(n => n.Ncc).Include(n => n.LIdNavigation).OrderBy(x => x.SpId), pageNumber, pageSize);

                    }
                    else
                    {
                        phieunhaphang.sanPhams = new PagedList<SanPham>(_context.SanPhams.Include(n => n.Ncc).Include(n => n.LIdNavigation).OrderBy(x => x.SpId), pageNumber, pageSize);

                    }

                }
                phieunhaphang.chitietPhieuNhap = chitietPhieunhaphangs;
                return View(phieunhaphang);
            }
            catch (Exception ex)
            {
                _notyf.Error("Lỗi tạo phiếu nhập");
                return RedirectToAction("Index");
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateReceipt(NhapHangViewModel phieuNhapViewModel)
        {
            var staffId = HttpContext.Session.GetString("Admin");
            if (staffId == null)
            {
                return RedirectToAction("Login", "AdminAccount");
            }
            try
            {
                using (var dbContext = new Chtc8Context()) // Thay "YourDbContext" bằng tên lớp DbContext của bạn
                {
                    PhieuNhapHang phieuNhapHang = new PhieuNhapHang()
                    {
                        NdId = int.Parse(staffId),
                        //NccId = phieuNhapViewModel.NccId,
                        PnhNgaynhap = DateTime.Now,
                    };
                    _context.Add(phieuNhapHang);
                    await _context.SaveChangesAsync();
                    decimal total = 0;
                    List<ChitietPhieuNhap> dataToAdd = chitietPhieunhaphangs;
                    foreach (ChitietPhieuNhap chitiet in dataToAdd)
                    {
                        var sp = await dbContext.SanPhams.FirstOrDefaultAsync(s => s.SpId == chitiet.Sp.SpId);
                        if (sp.SpSoluongton == null || sp.SpSoluongton == 0)
                        {
                            sp.SpSoluongton = chitiet.CtpnSoluong;
                        }
                        else
                        {
                            sp.SpSoluongton += chitiet.CtpnSoluong;
                        }
                        dbContext.Update(sp);
                        await dbContext.SaveChangesAsync();

                        chitiet.CtpnGiagoc = chitiet.CtpnGiagoc;
                        total += (decimal)(chitiet.CtpnGiagoc * chitiet.CtpnSoluong);
                        chitiet.SpId = chitiet.Sp.SpId;
                        chitiet.Sp = null;
                        chitiet.PnhId = phieuNhapHang.PnhId;
                        _context.Add(chitiet);
                    }
                    phieuNhapHang.PndDongia = total;
                    await _context.SaveChangesAsync();
                    _notyf.Success("Thêm phiếu nhập hàng thành công", 4);
                    HttpContext.Session.Set<List<ChitietPhieuNhap>>("chitietPhieunhaphangs", new List<ChitietPhieuNhap>());
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _notyf.Error("Không thể tạo phiếu nhập hàng", 4);
                return RedirectToAction("Create");
            }
        }



        [HttpGet]
		public IActionResult getConfirmRecordsPartial() // show modal
		{
            List<ChitietPhieuNhap> chitiets = chitietPhieunhaphangs;
            return Json(new { status = "success", chitiets });
            //return PartialView("ConfirmRecordsPartial", chitietPhieunhaphangs);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult updateChiTiets(int SpId, int amount, decimal price,bool allowZero = false)
        {
            List<ChitietPhieuNhap> chitiets = chitietPhieunhaphangs;
            try
            {
                ChitietPhieuNhap item = chitiets.SingleOrDefault(p => p.Sp.SpId == SpId);

                if (item == null)
                {
                    SanPham sp = _context.SanPhams.SingleOrDefault(p => p.SpId == SpId);
                    item = new ChitietPhieuNhap
                    {
                        Sp = sp,
                        CtpnSoluong = amount,
                        CtpnGiagoc = price
                    };
                    chitiets.Add(item);
                }
                else
                {
                    if (amount == 0 && !allowZero)
                    {
                        chitiets.Remove(item);
                    }
                    else
                    {
                        item.CtpnSoluong = amount;
                    }
                    if(price != item.CtpnGiagoc)
                    {
                        item.CtpnGiagoc = price;
                    }
                }

                // luu lai session
                HttpContext.Session.Set<List<ChitietPhieuNhap>>("chitietPhieunhaphangs", chitiets);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpGet]
        public async Task<JsonResult> DeleteSp(int? id)
        {
            //var sanPham = await _context.SanPhams.FirstOrDefaultAsync(m => m.SpId == id);
            //var nccTen = await _context.NhaCungCaps.Where(n => n.NccId == sanPham.NccId).Select(n => n.NccTen).FirstOrDefaultAsync();
            //var loaiTen = await _context.Loais.Where(l => l.LId == sanPham.LId).Select(l => l.LTenloai).FirstOrDefaultAsync();
            var phieunhaphang = await _context.PhieuNhapHangs.FirstOrDefaultAsync(p => p.PnhId == id);
            return Json(new { status = "success", phieunhaphang });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirm([FromBody] int receiptId)
        {
            if (_context.PhieuNhapHangs == null)
            {
                return Problem("Entity set 'ChtcContext.SanPhams'  is null.");
            }
            var phieunhaphang = await _context.PhieuNhapHangs.FindAsync(receiptId);
            if (phieunhaphang != null)
            {
                var chitietphieunhap = await _context.ChitietPhieuNhaps.Where(c => c.PnhId == receiptId).ToListAsync();
                for (var j = 0; j < chitietphieunhap.Count(); j++)
                {
                    _context.ChitietPhieuNhaps.Remove(chitietphieunhap[j]);
                }
                _context.PhieuNhapHangs.Remove(phieunhaphang);
            }

            await _context.SaveChangesAsync();
            return Json(new { status = "success" });
        }

        [HttpPost]
        public async Task<JsonResult> DeleteMany([FromBody] List<int> phieunhapIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var phieunhaps = await _context.PhieuNhapHangs.Where(p => phieunhapIds.Contains(p.PnhId)).ToListAsync();

                return Json(new { status = "success", phieunhaps });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cóH
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteManyConfirm([FromBody] List<int> phieunhapIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var chitietphieunhap = _context.ChitietPhieuNhaps.ToList();
                var phieunhaps = await _context.PhieuNhapHangs.Where(p => phieunhapIds.Contains(p.PnhId)).ToListAsync();
                for (var i = 0; i < phieunhaps.Count(); i++)
                {
                    for (var j = 0; j < chitietphieunhap.Count(); j++)
                    {
                        if (phieunhaps[i].PnhId == chitietphieunhap[j].PnhId)
                            _context.ChitietPhieuNhaps.Remove(chitietphieunhap[j]);
                    }
                    _context.PhieuNhapHangs.Remove(phieunhaps[i]);
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

    }
}
