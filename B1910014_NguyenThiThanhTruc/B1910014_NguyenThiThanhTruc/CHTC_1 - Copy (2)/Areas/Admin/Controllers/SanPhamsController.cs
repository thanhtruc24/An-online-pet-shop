using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CHTC_1.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using System.IO;
using PagedList.Core;
using Microsoft.AspNetCore.Authorization;
using NuGet.Protocol.Core.Types;
using Newtonsoft.Json;
using CHTC_1.Areas.Admin.Models;
using CHTC_1.ModelViews;

namespace CHTC_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class SanPhamsController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _environment;
        public SanPhamsController(Chtc8Context context, INotyfService notyf, IWebHostEnvironment environment)
        {
            _context = context;
            _notyf = notyf;
            _environment = environment;
        }

        // GET: Admin/SanPhams
        public async Task<IActionResult> Index(int page = 1,int LoaiID = 0)
        {
            
            var pageNumber = page ;
            var pageSize = 5;
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
  
            PagedList<SanPham> models = new PagedList<SanPham> (lsProducts.AsQueryable(), pageNumber, pageSize );
            ViewData["Loai"] = new SelectList(_context.Loais, "LId", "LTenloai");
            ViewBag.CurrentLoaiID = LoaiID;
            ViewBag.CurrentPage = pageNumber;
            return View(models);
            //var chtcContext = _context.SanPhams.Include(s => s.LIdNavigation).Include(s => s.Ncc);
            //return View(await chtcContext.ToListAsync());
        }
        [AllowAnonymous]
        public IActionResult Filtter(int LoaiID = 0)
        {
            var url = $"/Admin/SanPhams/Index?LoaiID={LoaiID}";
            if (LoaiID == 0)
            {
                url = $"/Admin/SanPhams/Index";
            }
            return Json(new { status = "success", redirectUrl = url });
        }               

        // GET: Admin/SanPhams/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _context.SanPhams == null)
        //    {
        //        return NotFound();
        //    }

        //    var sanPham = await _context.SanPhams
        //        .Include(s => s.LIdNavigation)
        //        .Include(s => s.Ncc)
        //        .FirstOrDefaultAsync(m => m.SpId == id);
        //    if (sanPham == null)
        //    {
        //        return NotFound();
        //    }

        //    return PartialView("DetailsPartial", sanPham);
        //}
        //public async Task<IActionResult> DetailsSp(int? id)
        //{

        //    var sanPham = await _context.SanPhams
        //        .FirstOrDefaultAsync(m => m.SpId == id);

        //    return PartialView("DetailsPartial", sanPham);
        //}
        [HttpGet]
        public async Task<JsonResult> DetailsSp(int? id)
        {
            // Trả về PartialView hiển thị thông tin chi tiết

            var sanPham = await _context.SanPhams.FirstOrDefaultAsync(m => m.SpId == id);
            var nccTen = await _context.NhaCungCaps.Where(n => n.NccId == sanPham.NccId).Select(n => n.NccTen).FirstOrDefaultAsync();
            var loaiTen = await _context.Loais.Where(l => l.LId == sanPham.LId).Select(l => l.LTenloai).FirstOrDefaultAsync();

            return Json(new { status = "success", sanPham, nccTen, loaiTen });
        }

        // GET: Admin/SanPhams/Create
        public IActionResult Create()
        {
            ViewData["Loai"] = new SelectList(_context.Loais, "LId", "LTenloai");
            ViewData["NhaCungCap"] = new SelectList(_context.NhaCungCaps, "NccId", "NccTen");
            return View();
        }

        // POST: Admin/SanPhams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpId,NccId,LId,SpTensp,SpGia,SpSoluong,SpMota,SpHinhanh")] SanPham sanPham, IFormFile ifile)
        {
            if (ModelState.IsValid)
            {
                string imgext = Path.GetExtension(ifile.FileName);
                if (imgext == ".jpg" || imgext == ".png")
                {
                    var uploadimg = Path.Combine(_environment.WebRootPath, "Images", ifile.FileName);
                    var stream = new FileStream(uploadimg, FileMode.Create);

                    await ifile.CopyToAsync(stream);
 
                    sanPham.SpHinhanh = ifile.FileName;
                    sanPham.SpSoluongton = 0;
                    _context.Add(sanPham);
                    await _context.SaveChangesAsync();
                    _notyf.Success("Thêm mới thành công");
                    return RedirectToAction(nameof(Index));
                }    
                
            }
            _notyf.Error("Thêm mới thất bại");
            ViewData["Loai"] = new SelectList(_context.Loais, "LId", "LTenloai", sanPham.LId);
            ViewData["NhaCungCap"] = new SelectList(_context.NhaCungCaps, "NccId", "NccTen", sanPham.NccId);
            return View(sanPham);
        }


// GET: Admin/SanPhams/Edit/5
    public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.SanPhams == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                return NotFound();
            }
            ViewData["Loai"] = new SelectList(_context.Loais, "LId", "LTenloai", sanPham.LId);
            ViewData["NhaCungCap"] = new SelectList(_context.NhaCungCaps, "NccId", "NccTen", sanPham.NccId);
            return View(sanPham);
        }

        // POST: Admin/SanPhams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SpId,NccId,LId,SpTensp,SpGia,SpSoluong,SpMota,SpHinhanh")] SanPham sanPham, IFormFile? ifile)
        {
            if (id != sanPham.SpId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ifile != null)
                    {
                        string imgext = Path.GetExtension(ifile.FileName);
                        if (imgext == ".jpg" || imgext == ".png")
                        {
                            var uploadimg = Path.Combine(_environment.WebRootPath, "Images", ifile.FileName);
                            var stream = new FileStream(uploadimg, FileMode.Create);

                            await ifile.CopyToAsync(stream);

                            sanPham.SpHinhanh = ifile.FileName;

                           
                        }
                    }
                    if(sanPham.SpHinhanh == null)
                    {
                        SanPham sp = _context.SanPhams.AsNoTracking().SingleOrDefault(s => s.SpId == sanPham.SpId);
                        sanPham.SpHinhanh = sp.SpHinhanh;
                    }

                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanPhamExists(sanPham.SpId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                _notyf.Success("Chỉnh sửa thành công");
                return RedirectToAction(nameof(Index));
            }
            _notyf.Error("Chỉnh sửa thất bại");
            ViewData["Loai"] = new SelectList(_context.Loais, "LId", "LTenloai", sanPham.LId);
            ViewData["NhaCungCap"] = new SelectList(_context.NhaCungCaps, "NccId", "NccTen", sanPham.NccId);
            return View(sanPham);
        }
        [HttpPost]
        public async Task<JsonResult> DeleteMany([FromBody] List<int> productIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var products = await _context.SanPhams.Where(p => productIds.Contains(p.SpId)).ToListAsync();

                return Json(new { status = "success", products });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteManyConfirm([FromBody] List<int> productIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var products = await _context.SanPhams.Where(p => productIds.Contains(p.SpId)).ToListAsync();
                for(var i = 0; i< products.Count(); i++)
                {
                    var binhluan = await _context.DanhGia.Where(b => b.Sp.SpId == products[i].SpId).ToListAsync();
                    for(var j =0; j< binhluan.Count(); j++)
                    {
                        _context.DanhGia.Remove(binhluan[j]);
                    }
                    _context.SanPhams.Remove(products[i]);
                }
                await _context.SaveChangesAsync();

                return Json(new { status = "success"});
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { status = "error", message = ex.Message });
            }
        }
        // GET: Admin/SanPhams/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.SanPhams == null)
        //    {
        //        return NotFound();
        //    }

        //    var sanPham = await _context.SanPhams
        //        .Include(s => s.LIdNavigation)
        //        .Include(s => s.Ncc)
        //        .FirstOrDefaultAsync(m => m.SpId == id);
        //    if (sanPham == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(sanPham);
        //}
        [HttpGet]
        public async Task<JsonResult> DeleteSp(int? id)
        {
            var sanPham = await _context.SanPhams.FirstOrDefaultAsync(m => m.SpId == id);
            var nccTen = await _context.NhaCungCaps.Where(n => n.NccId == sanPham.NccId).Select(n => n.NccTen).FirstOrDefaultAsync();
            var loaiTen = await _context.Loais.Where(l => l.LId == sanPham.LId).Select(l => l.LTenloai).FirstOrDefaultAsync();
            return Json(new { status = "success", sanPham, nccTen, loaiTen });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirm([FromBody]int productId)
        {
            if (_context.SanPhams == null)
            {
                return Problem("Entity set 'ChtcContext.SanPhams'  is null.");
            }
            var sanPham = await _context.SanPhams.FindAsync(productId);
            if (sanPham != null)
            {
                var binhluan = await _context.DanhGia.Where(b => b.Sp.SpId == productId).ToListAsync();
                for (var j = 0; j < binhluan.Count(); j++)
                {
                    _context.DanhGia.Remove(binhluan[j]);
                }
                _context.SanPhams.Remove(sanPham);
            }

            await _context.SaveChangesAsync();
            return Json(new { status = "success" });
        }

       

        // POST: Admin/SanPhams/Delete/5
        //[HttpPost, ActionName("DeleteSp")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.SanPhams == null)
        //    {
        //        return Problem("Entity set 'ChtcContext.SanPhams'  is null.");
        //    }
        //    var sanPham = await _context.SanPhams.FindAsync(id);
        //    if (sanPham != null)
        //    {
        //        _context.SanPhams.Remove(sanPham);
        //    }

        //    await _context.SaveChangesAsync();
        //    _notyf.Success("Xoá thành công");
        //    return RedirectToAction(nameof(Index));
        //}
        //[HttpGet]
        //public async Task<IActionResult> DeleteMany(string id)
        //{
        //    List<int> productIds = JsonConvert.DeserializeObject<List<int>>(id);
        //    return View(productIds);
        //}

        private bool SanPhamExists(int id)
        {
          return (_context.SanPhams?.Any(e => e.SpId == id)).GetValueOrDefault();
        }
    }
}
