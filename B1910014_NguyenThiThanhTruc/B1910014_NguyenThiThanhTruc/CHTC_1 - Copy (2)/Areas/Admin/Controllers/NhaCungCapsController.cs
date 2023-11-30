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
    [Authorize(Roles = "Admin,Staff")]
    [Authorize]
    public class NhaCungCapsController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
        public NhaCungCapsController(Chtc8Context context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // GET: Admin/NhaCungCaps
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var lsSupliers = _context.NhaCungCaps.AsNoTracking().OrderByDescending(x => x.NccId);
            PagedList<NhaCungCap> models = new PagedList<NhaCungCap>(lsSupliers, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
            //return _context.NhaCungCaps != null ? 
            //              View(await _context.NhaCungCaps.ToListAsync()) :
            //              Problem("Entity set 'ChtcContext.NhaCungCaps'  is null.");
        }

        // GET: Admin/NhaCungCaps/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _context.NhaCungCaps == null)
        //    {
        //        return NotFound();
        //    }

        //    var nhaCungCap = await _context.NhaCungCaps
        //        .FirstOrDefaultAsync(m => m.NccId == id);
        //    if (nhaCungCap == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(nhaCungCap);
        //}

        [HttpGet]
        public async Task<JsonResult> DetailSupplier(int? id)
        {
            // Trả về PartialView hiển thị thông tin chi tiết

            var nhaCungCap = await _context.NhaCungCaps
                .FirstOrDefaultAsync(m => m.NccId == id);
            return Json(new { status = "success", nhaCungCap });
        }

        // GET: Admin/NhaCungCaps/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/NhaCungCaps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NccId,NccTen")] NhaCungCap nhaCungCap)
        {
            if (ModelState.IsValid)
            {
                var model = _context.NhaCungCaps.SingleOrDefault(x => x.NccTen == nhaCungCap.NccTen);
                if (model == null)
                {
                    _context.Add(nhaCungCap);
                    await _context.SaveChangesAsync();
                    _notyf.Success("Thêm mới thành công");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _notyf.Error("Nhà cung cấp đã tồn tại");
                    return View(nhaCungCap);
                }
                
            }
            _notyf.Error("Thêm mới thất bại");
            return View(nhaCungCap);
        }

        // GET: Admin/NhaCungCaps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.NhaCungCaps == null)
            {
                return NotFound();
            }

            var nhaCungCap = await _context.NhaCungCaps.FindAsync(id);
            if (nhaCungCap == null)
            {
                return NotFound();
            }
            return View(nhaCungCap);
        }

        // POST: Admin/NhaCungCaps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NccId,NccTen")] NhaCungCap nhaCungCap)
        {
            if (id != nhaCungCap.NccId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhaCungCap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhaCungCapExists(nhaCungCap.NccId))
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
            return View(nhaCungCap);
        }

        [HttpGet]
        public async Task<JsonResult> DeleteSupplier(int? id)
        {
            var nhaCungCap = await _context.NhaCungCaps
               .FirstOrDefaultAsync(m => m.NccId == id);
            return Json(new { status = "success", nhaCungCap });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSupplierConfirm([FromBody] int supplierId)
        {
            if (_context.NhaCungCaps == null)
            {
                return Problem("Entity set 'ChtcContext.NhaCungCaps'  is null.");
            }
            var ncc = await _context.NhaCungCaps.FindAsync(supplierId);
            if (ncc != null)
            {
                var sp = _context.SanPhams.Where(s => s.NccId == supplierId).FirstOrDefault();
                if(sp != null)
                {
                    return Json(new { status = "error" });
                }
                else
                {
                    _context.NhaCungCaps.Remove(ncc);
                    await _context.SaveChangesAsync();
                    return Json(new { status = "success" });
                }    
               
            }
            return Json(new { status = "error" });

        }

        [HttpPost]
        public async Task<JsonResult> DeleteMany([FromBody] List<int> supplierIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var suppliers = await _context.NhaCungCaps.Where(p => supplierIds.Contains(p.NccId)).ToListAsync();

                return Json(new { status = "success", suppliers });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteManyConfirm([FromBody] List<int> supplierIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var suppliers = await _context.NhaCungCaps.Where(p => supplierIds.Contains(p.NccId)).ToListAsync();
                for (var i = 0; i < suppliers.Count(); i++)
                {
                    _context.NhaCungCaps.Remove(suppliers[i]);
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

        // GET: Admin/NhaCungCaps/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.NhaCungCaps == null)
        //    {
        //        return NotFound();
        //    }

        //    var nhaCungCap = await _context.NhaCungCaps
        //        .FirstOrDefaultAsync(m => m.NccId == id);
        //    if (nhaCungCap == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(nhaCungCap);
        //}

        // POST: Admin/NhaCungCaps/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.NhaCungCaps == null)
        //    {
        //        return Problem("Entity set 'ChtcContext.NhaCungCaps'  is null.");
        //    }
        //    var nhaCungCap = await _context.NhaCungCaps.FindAsync(id);
        //    if (nhaCungCap != null)
        //    {
        //        _context.NhaCungCaps.Remove(nhaCungCap);
        //    }

        //    await _context.SaveChangesAsync();
        //    _notyf.Success("Xoá thành công");
        //    return RedirectToAction(nameof(Index));
        //}

        private bool NhaCungCapExists(int id)
        {
          return (_context.NhaCungCaps?.Any(e => e.NccId == id)).GetValueOrDefault();
        }
    }
}
