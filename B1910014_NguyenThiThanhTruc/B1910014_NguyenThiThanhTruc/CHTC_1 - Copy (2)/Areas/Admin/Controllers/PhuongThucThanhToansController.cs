using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CHTC_1.Models;
using Microsoft.AspNetCore.Authorization;

namespace CHTC_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PhuongThucThanhToansController : Controller
    {
        private readonly Chtc8Context _context;

        public PhuongThucThanhToansController(Chtc8Context context)
        {
            _context = context;
        }

        // GET: Admin/PhuongThucThanhToans
        public async Task<IActionResult> Index()
        {
              return _context.PhuongThucThanhToans != null ? 
                          View(await _context.PhuongThucThanhToans.ToListAsync()) :
                          Problem("Entity set 'ChtcContext.PhuongThucThanhToans'  is null.");
        }

        // GET: Admin/PhuongThucThanhToans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PhuongThucThanhToans == null)
            {
                return NotFound();
            }

            var phuongThucThanhToan = await _context.PhuongThucThanhToans
                .FirstOrDefaultAsync(m => m.PtttId == id);
            if (phuongThucThanhToan == null)
            {
                return NotFound();
            }

            return View(phuongThucThanhToan);
        }

        // GET: Admin/PhuongThucThanhToans/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/PhuongThucThanhToans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PtttId,PtttTen")] PhuongThucThanhToan phuongThucThanhToan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phuongThucThanhToan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(phuongThucThanhToan);
        }

        // GET: Admin/PhuongThucThanhToans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PhuongThucThanhToans == null)
            {
                return NotFound();
            }

            var phuongThucThanhToan = await _context.PhuongThucThanhToans.FindAsync(id);
            if (phuongThucThanhToan == null)
            {
                return NotFound();
            }
            return View(phuongThucThanhToan);
        }

        // POST: Admin/PhuongThucThanhToans/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PtttId,PtttTen")] PhuongThucThanhToan phuongThucThanhToan)
        {
            if (id != phuongThucThanhToan.PtttId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phuongThucThanhToan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhuongThucThanhToanExists(phuongThucThanhToan.PtttId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(phuongThucThanhToan);
        }

        // GET: Admin/PhuongThucThanhToans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PhuongThucThanhToans == null)
            {
                return NotFound();
            }

            var phuongThucThanhToan = await _context.PhuongThucThanhToans
                .FirstOrDefaultAsync(m => m.PtttId == id);
            if (phuongThucThanhToan == null)
            {
                return NotFound();
            }

            return View(phuongThucThanhToan);
        }

        // POST: Admin/PhuongThucThanhToans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PhuongThucThanhToans == null)
            {
                return Problem("Entity set 'ChtcContext.PhuongThucThanhToans'  is null.");
            }
            var phuongThucThanhToan = await _context.PhuongThucThanhToans.FindAsync(id);
            if (phuongThucThanhToan != null)
            {
                _context.PhuongThucThanhToans.Remove(phuongThucThanhToan);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhuongThucThanhToanExists(int id)
        {
          return (_context.PhuongThucThanhToans?.Any(e => e.PtttId == id)).GetValueOrDefault();
        }
    }
}
