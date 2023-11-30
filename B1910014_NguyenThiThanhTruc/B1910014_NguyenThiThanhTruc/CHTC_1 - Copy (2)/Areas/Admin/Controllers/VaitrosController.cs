using AspNetCoreHero.ToastNotification.Abstractions;
using CHTC_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;

namespace CHTC_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin,Staff")]
    public class VaitrosController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
        public VaitrosController(Chtc8Context context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // GET: Admin/Loais
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var lsTypes = _context.VaiTros.AsNoTracking().OrderByDescending(x => x.VtId);
            PagedList<VaiTro> models = new PagedList<VaiTro>(lsTypes, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);

        }

        // GET: Admin/Loais/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _context.VaiTros == null)
        //    {
        //        return NotFound();
        //    }

        //    var roles = await _context.VaiTros
        //        .FirstOrDefaultAsync(m => m.VtId == id);
        //    if (roles == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(roles);
        //}

        [HttpGet]
        public async Task<JsonResult> DetailRole(int? id)
        {
            // Trả về PartialView hiển thị thông tin chi tiết

            var role = await _context.VaiTros
                .FirstOrDefaultAsync(m => m.VtId == id);
            return Json(new { status = "success", role });
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
        public async Task<IActionResult> Create([Bind("VtId,VtTenvaitro")] VaiTro role)
        {
            if (ModelState.IsValid)
            {
                _context.Add(role);
                await _context.SaveChangesAsync();
                _notyf.Success("Thêm vai trò thành công");
                return RedirectToAction(nameof(Index));
            }
            _notyf.Error("Thêm loại mới thất bại");
            return View(role);
        }

        // GET: Admin/Loais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VaiTros == null)
            {
                return NotFound();
            }

            var role = await _context.VaiTros.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Admin/Loais/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VtId,VtTenvaitro")] VaiTro role)
        {
            if (id != role.VtId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiExists(role.VtId))
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
            return View(role);
        }

        [HttpGet]
        public async Task<JsonResult> DeleteRole(int? id)
        {
           var role = await _context.VaiTros
               .FirstOrDefaultAsync(m => m.VtId == id);
            return Json(new { status = "success", role });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRoleConfirm([FromBody] int roleId)
        {
            if (_context.VaiTros == null)
            {
                return Problem("Entity set 'ChtcContext.NhaCungCaps'  is null.");
            }
            var role = await _context.VaiTros.FindAsync(roleId);
            if (role != null)
            {
                var taikhoan = _context.TaiKhoans.Where(s => s.VtId == roleId).FirstOrDefault();
                if (taikhoan != null)
                {
                    return Json(new { status = "error" });
                }
                else
                {
                    _context.VaiTros.Remove(role);
                    await _context.SaveChangesAsync();
                    return Json(new { status = "success" });
                }

            }
            return Json(new { status = "error" });

        }

        [HttpPost]
        public async Task<JsonResult> DeleteMany([FromBody] List<int> roleIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var roles = await _context.VaiTros.Where(p => roleIds.Contains(p.VtId)).ToListAsync();

                return Json(new { status = "success", roles });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteManyConfirm([FromBody] List<int> roleIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var roles = await _context.VaiTros.Where(p => roleIds.Contains(p.VtId)).ToListAsync();
                for (var i = 0; i < roles.Count(); i++)
                {
                    _context.VaiTros.Remove(roles[i]);
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
        //    if (id == null || _context.VaiTros == null)
        //    {
        //        return NotFound();
        //    }

        //    var role = await _context.VaiTros
        //        .FirstOrDefaultAsync(m => m.VtId == id);
        //    if (role == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(role);
        //}

        //// POST: Admin/Loais/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.VaiTros == null)
        //    {
        //        return Problem("Entity set 'ChtcContext.Loais'  is null.");
        //    }
        //    var role = await _context.VaiTros.FindAsync(id);
        //    if (role != null)
        //    {
        //        _context.VaiTros.Remove(role);
        //    }

        //    await _context.SaveChangesAsync();
        //    _notyf.Success("Xoá thành công");
        //    return RedirectToAction(nameof(Index));
        //}



        private bool LoaiExists(int id)
        {
            return (_context.VaiTros?.Any(e => e.VtId == id)).GetValueOrDefault();
        }
    }
}
