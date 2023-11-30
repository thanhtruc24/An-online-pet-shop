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

namespace CHTC_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin,Staff")]
    [Authorize]
    public class LoaisController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
        public LoaisController(Chtc8Context context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // GET: Admin/Loais
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var lsTypes = _context.Loais.AsNoTracking().OrderByDescending(x => x.LId);
            PagedList<Loai> models = new PagedList<Loai>(lsTypes, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);

            //return _context.Loais != null ? 
            //              View(await _context.Loais.ToListAsync()) :
            //              Problem("Entity set 'ChtcContext.Loais'  is null.");
        }

        [HttpGet]
        public async Task<JsonResult> DetailsCategory(int? id)
        {
            // Trả về PartialView hiển thị thông tin chi tiết

            var loai = await _context.Loais.FirstOrDefaultAsync(m => m.LId == id);
            return Json(new { status = "success", loai });
        }

        // GET: Admin/Loais/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Loais/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LId,LTenloai")] Loai loai)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loai);
                await _context.SaveChangesAsync();
                _notyf.Success("Thêm loại mới thành công");
                return RedirectToAction(nameof(Index));
            }
            _notyf.Error("Thêm loại mới thất bại");
            return View(loai);
        }

        // GET: Admin/Loais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Loais == null)
            {
                return NotFound();
            }

            var loai = await _context.Loais.FindAsync(id);
            if (loai == null)
            {
                return NotFound();
            }
            return View(loai);
        }

        // POST: Admin/Loais/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LId,LTenloai")] Loai loai)
        {
            if (id != loai.LId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loai);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiExists(loai.LId))
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
            return View(loai);
        }
        [HttpGet]
        public async Task<JsonResult> DeleteCate(int? id)
        {
            var loai = await _context.Loais.FirstOrDefaultAsync(m => m.LId == id);
            return Json(new { status = "success", loai });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCateConfirm([FromBody] int categoryId)
        {
            if (_context.Loais == null)
            {
                return Problem("Entity set 'ChtcContext.NhaCungCaps'  is null.");
            }
            var loai = await _context.Loais.FindAsync(categoryId);
            if (loai != null)
            {
                var sp = _context.SanPhams.Where(s => s.LId == categoryId).FirstOrDefault();
                if (sp != null)
                {
                    return Json(new { status = "error" });
                }
                else
                {
                    _context.Loais.Remove(loai);
                    await _context.SaveChangesAsync();
                    return Json(new { status = "success" });
                }

            }
            return Json(new { status = "error" });
        }

        [HttpPost]
        public async Task<JsonResult> DeleteMany([FromBody] List<int> categoryIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var categories = await _context.Loais.Where(p => categoryIds.Contains(p.LId)).ToListAsync();

                return Json(new { status = "success", categories });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteManyConfirm([FromBody] List<int> categoryIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var categories = await _context.Loais.Where(p => categoryIds.Contains(p.LId)).ToListAsync();
                for (var i = 0; i < categories.Count(); i++)
                {
                    _context.Loais.Remove(categories[i]);
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
        // GET: Admin/Loais/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.Loais == null)
        //    {
        //        return NotFound();
        //    }

        //    var loai = await _context.Loais
        //        .FirstOrDefaultAsync(m => m.LId == id);
        //    if (loai == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(loai);
        //}

        //// POST: Admin/Loais/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.Loais == null)
        //    {
        //        return Problem("Entity set 'ChtcContext.Loais'  is null.");
        //    }
        //    var loai = await _context.Loais.FindAsync(id);
        //    if (loai != null)
        //    {
        //        _context.Loais.Remove(loai);
        //    }

        //    await _context.SaveChangesAsync();
        //    _notyf.Success("Xoá thành công");
        //    return RedirectToAction(nameof(Index));
        //}

        private bool LoaiExists(int id)
        {
          return (_context.Loais?.Any(e => e.LId == id)).GetValueOrDefault();
        }
    }
}
