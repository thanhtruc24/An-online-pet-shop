using AspNetCoreHero.ToastNotification.Abstractions;
using CHTC_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CHTC_1.Controllers
{
    public class NguoiDungController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _environment;
        public NguoiDungController(Chtc8Context context, INotyfService notyf, IWebHostEnvironment environment)
        {
            _context = context;
            _notyf = notyf;
            _environment = environment;
        }

        public IActionResult ProfileCustomer()
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                var khachhang = _context.TaiKhoans.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
                if (khachhang != null)
                {
                    var models = _context.TaiKhoans
                        .Include(n => n.Nd)
                        .Where(x => x.NdId == khachhang.NdId)
                        .SingleOrDefault();
                    return View(models);
                }

            }
            return RedirectToAction("Accounts/Login");
        }

        public async Task<IActionResult> EditProfile(int? id)
        {
            if (id == null || _context.NguoiDungs == null)
            {
                return NotFound();
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null)
            {
                return NotFound();
            }
            return View(nguoiDung);
        }

        // POST: Admin/NguoiDungs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(int id, [Bind("NdId,PnhId,NdHoten,NdHinhanh,NdSdt,NdDiachi,NdGioitinh")] NguoiDung nguoiDung, IFormFile? ifile)
        {
            if (id != nguoiDung.NdId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if(ifile != null)
                    {
                        string imgext = Path.GetExtension(ifile.FileName);
                        if (imgext == ".jpg" || imgext == ".png")
                        {
                            var uploadimg = Path.Combine(_environment.WebRootPath, "Images", ifile.FileName);
                            var stream = new FileStream(uploadimg, FileMode.Create);

                            await ifile.CopyToAsync(stream);

                            nguoiDung.NdHinhanh = ifile.FileName;
                        }
                    }
                    if (nguoiDung.NdHinhanh == null)
                    {
                        NguoiDung nguoidungdb = _context.NguoiDungs.AsNoTracking().SingleOrDefault(n => n.NdId == nguoiDung.NdId);
                        nguoiDung.NdHinhanh = nguoidungdb.NdHinhanh;
                    }
                    _context.Update(nguoiDung);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NguoiDungExists(nguoiDung.NdId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                _notyf.Success("Chỉnh sửa thành công");
                return RedirectToAction(nameof(ProfileCustomer));
            }
            _notyf.Error("Chỉnh sửa thất bại");
            return View(nguoiDung);
        }

        private bool NguoiDungExists(int id)
        {
            return (_context.NguoiDungs?.Any(e => e.NdId == id)).GetValueOrDefault();
        }

    }
}
