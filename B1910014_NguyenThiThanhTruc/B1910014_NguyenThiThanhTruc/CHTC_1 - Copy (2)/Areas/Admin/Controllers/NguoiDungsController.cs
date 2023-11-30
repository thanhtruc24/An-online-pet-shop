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
using CHTC_1.Mail;
using CHTC_1.ModelViews;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using CHTC_1.Areas.Admin.Models;
using Gravatar;
using CHTC_1.Avatar;
using FluentEmail.Core;


namespace CHTC_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NguoiDungsController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _environment;
        private readonly EmailService _emailService;
        public NguoiDungsController(Chtc8Context context, INotyfService notyf, IWebHostEnvironment environment, EmailService email)
        {
            _context = context;
            _notyf = notyf;
            _environment = environment;
            _emailService = email;
        }

        // GET: Admin/NguoiDungs
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var lsAccounts = _context.TaiKhoans.AsNoTracking().Include(s => s.Vt).Include(n => n.Nd).OrderByDescending(x => x.NdId);
            PagedList<TaiKhoan> models = new PagedList<TaiKhoan>(lsAccounts, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
            //var chtcContext = _context.NguoiDungs.Include(n => n.Qsd);
            //return View(await chtcContext.ToListAsync());
        }

        // GET: Admin/NguoiDungs/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _context.TaiKhoans == null)
        //    {
        //        return NotFound();
        //    }

        //    var nguoiDung = await _context.TaiKhoans
        //        .Include(n => n.Nd)
        //        .Include(v => v.Vt)
        //        .FirstOrDefaultAsync(m => m.NdId == id);
        //    if (nguoiDung == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(nguoiDung);
        //}

        [HttpGet]
        public async Task<JsonResult> DetailsUser(int? id)
        {
            // Trả về PartialView hiển thị thông tin chi tiết

            var taikhoan = await _context.TaiKhoans.FirstOrDefaultAsync(m => m.NdId == id);
            //var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(n => n.NdId == id);
            var userName = await _context.NguoiDungs.Where( n => n.NdId == id).Select(v => v.NdHoten).FirstOrDefaultAsync();
            var userAddress = await _context.NguoiDungs.Where( n => n.NdId == id).Select(v => v.NdDiachi).FirstOrDefaultAsync();
            var userPhone = await _context.NguoiDungs.Where(n => n.NdId == id).Select(v => v.NdSdt).FirstOrDefaultAsync();
            var userGender = await _context.NguoiDungs.Where(n => n.NdId == id).Select(v => v.NdGioitinh).FirstOrDefaultAsync();
            var userImage = await _context.NguoiDungs.Where(n => n.NdId == id).Select(v => v.NdHinhanh).FirstOrDefaultAsync();
            var vaitro = await _context.VaiTros.Where(v => v.VtId == taikhoan.VtId).Select(v => v.VtTenvaitro).FirstOrDefaultAsync();

            return Json(new { status = "success", taikhoan, vaitro, userName, userAddress, userGender, userPhone, userImage });
        }


        // GET: Admin/NguoiDungs/Create
        //public IActionResult Create()
        //{
        //    ViewData["Vaitrotaikhoan"] = new SelectList(_context.VaiTros, "VtId", "VtTenvaitro");
        //    return View();
        //}


        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreateUser()
        {
            ViewData["Vaitrotaikhoan"] = new SelectList(_context.VaiTros, "VtId", "VtTenvaitro");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser(CreateAccount taikhoan)
        {
                if (ModelState.IsValid)
                {
                    
                    TaiKhoan khachhang = new TaiKhoan
                    {
                        TkEmail = taikhoan.Email.Trim().ToLower(),
                        TkMatkhau = taikhoan.Password
                    };

                    try
                    {
                    
                    var nguoidung = new NguoiDung();
                      
                            nguoidung.NdHoten = taikhoan.FullName;
                            nguoidung.NdDiachi = taikhoan.Address;
                            nguoidung.NdSdt = taikhoan.Phone;
                            nguoidung.NdHinhanh = "avt1.jpg";
                            nguoidung.NdGioitinh = 2;
                        _context.Add(nguoidung);
                        await _context.SaveChangesAsync();

                        khachhang.NdId = nguoidung.NdId;
                        khachhang.VtId = taikhoan.RoleId;
                        _context.Add(khachhang);
                        await _context.SaveChangesAsync();
                    
                        await _emailService.SendEmailAsync(taikhoan.Email,"Chào mừng bạn trở thành thành viên của DoCashop",
                        "<html><body>" +
                        "<p>Xin chào,</p>" +
                        "<p>Chào mừng bạn trở thành thành viên của DoCa. Chúng tôi đã đính kèm tài khoản dưới đây.</p>" +
                        "<ul>" +
                        "<li>Tài khoản: " + taikhoan.Email + "</li>" +
                        "<li>Mật khẩu: " + taikhoan.Password + "</li>" +
                        "</ul>" +
                        "</body></html>");
                        _notyf.Success("Tạo tài khoản thành công");
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                       
                    }
                }
            _notyf.Error("Tạo tài khoản thất bại");
            ViewData["Vaitrotaikhoan"] = new SelectList(_context.VaiTros, "VtId", "VtTenvaitro", taikhoan.RoleId);
            return View(taikhoan);
        }

        // POST: Admin/NguoiDungs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //     [HttpPost]
        //     [ValidateAntiForgeryToken]
        //     public async Task<IActionResult> Create([Bind("TkEmail,VtId,NdId,TkMatkhau,Nd.NdHoten,Nd.NdDiachi,Nd.NdSdt,Nd.NdHinhanh")] TaiKhoan nguoiDung, IFormFile ifile)
        //     {
        //         if (ModelState.IsValid)
        //         {
        //             string imgext = Path.GetExtension(ifile.FileName);
        //             if (imgext == ".jpg" || imgext == ".png")
        //             {
        //                 var uploadimg = Path.Combine(_environment.WebRootPath, "Images", ifile.FileName);
        //                 var stream = new FileStream(uploadimg, FileMode.Create);

        //                 await ifile.CopyToAsync(stream);
        //                 var hotennd = nguoiDung.Nd.NdHoten; 
        //                 NguoiDung nguoidung = new NguoiDung
        //                 {
        //                     NdHoten = nguoiDung.Nd.NdHoten,
        //                     NdDiachi = nguoiDung.Nd.NdDiachi,
        //                     NdSdt = nguoiDung.Nd.NdSdt,
        //                     NdHinhanh = ifile.FileName
        //                 };

        //                 _context.Add(nguoidung);
        //                 await _context.SaveChangesAsync();
        //                  nguoiDung.NdId = nguoidung.NdId;
        //                 //await _context.SaveChangesAsync();
        //                 //_notyf.Success("Thêm mới thành công");
        //                 //return RedirectToAction(nameof(Index));
        //             }
        //             _context.Add(nguoiDung);
        //	await _context.SaveChangesAsync();
        //             await _emailService.SendEmailAsync(nguoiDung.TkEmail, "Chào mừng bạn trở thành thành viên của Petshop", "Xin chào. Tài khoản: "+nguoiDung.TkEmail + "Mật khẩu: " + nguoiDung.TkMatkhau);
        //             _notyf.Success("Thêm mới thành công");
        //	return RedirectToAction(nameof(Index));

        //}
        //         _notyf.Error("Thêm mới thất bại");
        //         ViewData["Vaitrotaikhoan"] = new SelectList(_context.VaiTros, "VtId", "VtTenvaitro", nguoiDung.VtId);
        //         return View(nguoiDung);
        //     }

        // GET: Admin/NguoiDungs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.NguoiDungs == null)
            {
                return NotFound();
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            //var taikhoan = await _context.TaiKhoans.FirstOrDefaultAsync( t => t.NdId == id);
            if (nguoiDung == null)
            {
                return NotFound();
            }
			//ViewData["Vaitrotaikhoan"] = new SelectList(_context.VaiTros, "VtId", "VtTenvaitro", taikhoan.VtId);
			return View(nguoiDung);
        }

        // POST: Admin/NguoiDungs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NdId,PnhId,NdHoten,NdHinhanh,NdSdt,NdDiachi,NdGioitinh")] NguoiDung nguoiDung, IFormFile? ifile)
        {
            

            if (ModelState.IsValid)
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
                        if(nguoiDung.NdHinhanh == null)
                        {
                            NguoiDung nguoidungDb = _context.NguoiDungs.AsNoTracking().FirstOrDefault(n => n.NdId == nguoiDung.NdId);
                            nguoiDung.NdHinhanh = nguoidungDb.NdHinhanh;
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
                    return RedirectToAction(nameof(Index));
                }
                _notyf.Error("Chỉnh sửa thất bại");
                //ViewData["Loai"] = new SelectList(_context.Loais, "LId", "LTenloai", sanPham.LId);
                //ViewData["NhaCungCap"] = new SelectList(_context.NhaCungCaps, "NccId", "NccTen", sanPham.NccId);
                return View(nguoiDung);
            }
            _notyf.Error("Chỉnh sửa thất bại");
			//ViewData["Vaitrotaikhoan"] = new SelectList(_context.VaiTros, "VtId", "VtTenvaitro", nguoiDung.VtId);
			return View(nguoiDung);
        }

        // GET: Admin/NguoiDungs/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.TaiKhoans == null)
        //    {
        //        return NotFound();
        //    }

        //    var nguoiDung = await _context.TaiKhoans
        //        .Include(n => n.Nd)
        //        .Include(v => v.Vt)
        //        .FirstOrDefaultAsync(m => m.NdId == id);
        //    if (nguoiDung == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(nguoiDung);
        //}

        //// POST: Admin/NguoiDungs/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.NguoiDungs == null)
        //    {
        //        return Problem("Entity set 'ChtcContext.NguoiDungs'  is null.");
        //    }
        //    var nguoiDung = await _context.NguoiDungs.FindAsync(id);
        //    if (nguoiDung != null)
        //    {
        //        var taikhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.NdId == id);
        //        _context.TaiKhoans.Remove(taikhoan);
        //        _context.NguoiDungs.Remove(nguoiDung);
        //    }
            
        //    await _context.SaveChangesAsync();
        //    _notyf.Success("Xoá thành công");
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpGet]
        public async Task<JsonResult> DeleteUser(int? id)
        {
            var taikhoan = await _context.TaiKhoans.FirstOrDefaultAsync(m => m.NdId == id);
            //var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(n => n.NdId == id);
            var userName = await _context.NguoiDungs.Where(n => n.NdId == id).Select(v => v.NdHoten).FirstOrDefaultAsync();
            var userAddress = await _context.NguoiDungs.Where(n => n.NdId == id).Select(v => v.NdDiachi).FirstOrDefaultAsync();
            var userPhone = await _context.NguoiDungs.Where(n => n.NdId == id).Select(v => v.NdSdt).FirstOrDefaultAsync();
            var userGender = await _context.NguoiDungs.Where(n => n.NdId == id).Select(v => v.NdGioitinh).FirstOrDefaultAsync();
            var userImage = await _context.NguoiDungs.Where(n => n.NdId == id).Select(v => v.NdHinhanh).FirstOrDefaultAsync();
            var vaitro = await _context.VaiTros.Where(v => v.VtId == taikhoan.VtId).Select(v => v.VtTenvaitro).FirstOrDefaultAsync();

            return Json(new { status = "success", taikhoan, vaitro, userName, userAddress, userGender, userPhone, userImage });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserConfirm([FromBody] int userId)
        {
            if (_context.NguoiDungs == null)
            {
                return Problem("Entity set 'ChtcContext.SanPhams'  is null.");
            }
            var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
            if (nguoiDung != null)
            {
                var taikhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.NdId == userId);
                _context.TaiKhoans.Remove(taikhoan);
                _context.NguoiDungs.Remove(nguoiDung);
            }

            await _context.SaveChangesAsync();
            return Json(new { status = "success" });
        }



        [HttpPost]
        public async Task<JsonResult> DeleteMany([FromBody] List<int> userIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var users = await _context.NguoiDungs.Where(p => userIds.Contains(p.NdId)).ToListAsync();

                return Json(new { status = "success", users });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteManyConfirm([FromBody] List<int> userIds)
        {
            try
            {
                // Lấy thông tin sản phẩm dựa trên danh sách productIds
                var users = await _context.NguoiDungs.Where(p => userIds.Contains(p.NdId)).ToListAsync();
                for (var i = 0; i < users.Count(); i++)
                {
                    var taikhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.NdId == users[i].NdId);
                    _context.TaiKhoans.Remove(taikhoan);
                    await _context.SaveChangesAsync();
                    _context.NguoiDungs.Remove(users[i]);
                    await _context.SaveChangesAsync();

                }
                

                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { status = "error", message = ex.Message });
            }
        }

        private bool NguoiDungExists(int id)
        {
          return (_context.TaiKhoans?.Any(e => e.NdId == id)).GetValueOrDefault();
        }
    }
}
