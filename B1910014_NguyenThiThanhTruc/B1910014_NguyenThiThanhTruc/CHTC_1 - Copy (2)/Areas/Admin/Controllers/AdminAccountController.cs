using AspNetCoreHero.ToastNotification.Abstractions;
using CHTC_1.Areas.Admin.Models;
using CHTC_1.Helper;
using CHTC_1.Mail;
using CHTC_1.Models;
using CHTC_1.ModelViews;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CHTC_1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAccountController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
		private readonly IWebHostEnvironment _environment;
        private readonly EmailService _emailService;
        public AdminAccountController(Chtc8Context context, INotyfService notyf, IWebHostEnvironment environment, EmailService emailService)
        {
            _context = context;
            _notyf = notyf;
            _environment = environment;
            _emailService = emailService;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}
        public IActionResult Login(string returnUrl = null)
        
        {
            var taikhoanid = HttpContext.Session.GetString("Admin");
            if (taikhoanid != null)
            {
               return RedirectToAction("Index", "Home", new { Area = "Admin" });
            }
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> Login(AdminLoginViewModel admin, string returnUrl = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    TaiKhoan ad = _context.TaiKhoans
                    .Include(p => p.Vt)
                    .SingleOrDefault(p => p.TkEmail.ToLower() == admin.Email.ToLower().Trim());
                    bool isEmail = Utilities.IsValidEmail(admin.Email);

                    if (ad == null)
                    {
                        _notyf.Error("Thông tin đăng nhập chưa chính xác");
                        return RedirectToAction("Login", "AdminAccount", new { Area = "Admin" });
                    }              

                    string pass = admin.Password;
                    if (ad.TkMatkhau != pass)
                    {
                        _notyf.Error("Thông tin đăng nhập chưa chính xác");
                        return View(admin);
                    }
                    if(ad.VtId == 1)
                    {
                        //Luu Session MaKh
                        HttpContext.Session.SetString("Admin", ad.NdId.ToString());
                        var adminID = HttpContext.Session.GetString("Admin");
                    }
                    else
                    {
                        //Luu Session MaKh
                        HttpContext.Session.SetString("Staff", ad.NdId.ToString());
                        var staffID = HttpContext.Session.GetString("Staff");
                    }

                    
                    

                    //Identity
                    var userClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, ad.TkEmail),
                        new Claim("AccountId", ad.NdId.ToString()),
                        new Claim("RoleId", ad.VtId.ToString()),
                        new Claim(ClaimTypes.Role, ad.Vt.VtTenvaitro)
                    };
                    var grandmaIdentity = new ClaimsIdentity(userClaims, "User Identity");
                    var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity });
                    await HttpContext.SignInAsync(userPrincipal);
                    _notyf.Success("Đăng nhập thành công");

                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction("Index", "Home", new { Area = "Admin" });
                    }
                    else
                    {
                        return Redirect(returnUrl);
                    }
                }
            }
            catch
            {
                return RedirectToAction("Login", "AdminAccount", new { Area = "Admin" });
            }
            return View(admin);
        }


        [HttpGet]
        public IActionResult AdminLogout()
        {
            try
            {
                HttpContext.SignOutAsync();
                HttpContext.Session.Remove("Admin");
				HttpContext.Session.Remove("Staff");
				return RedirectToAction("Login", "AdminAccount", new { Area = "Admin" });
            }
            catch
            {
                return RedirectToAction("Login", "AdminAccount", new { Area = "Admin" });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AdminChangePassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AdminChangePassword(ChangePwAdmin model)
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("Admin");
                if (taikhoanID == null)
                {
                    return RedirectToAction("Login", "AdminAccount", new { Area = "Admin"});
                }
                if (ModelState.IsValid)
                {
                    var taikhoan = _context.TaiKhoans.Find(Convert.ToInt32(taikhoanID));
                    if (taikhoan == null) return RedirectToAction("Login", "AdminAccount", new { Area = "Admin" });
                    var pass = model.PasswordNow.Trim();
                    {
                        string passnew = model.Password.Trim();
                        taikhoan.TkMatkhau = passnew;
                        _context.Update(taikhoan);
                        _context.SaveChanges();
                        _notyf.Success("Đổi mật khẩu thành công");
                        return RedirectToAction("Index", "Home", new { Area = "Admin"});
                    }
                }
            }
            catch
            {
                _notyf.Error("Thay đổi mật khẩu không thành công");
                return RedirectToAction("Index", "Home", new { Area ="Admin"});
            }
            _notyf.Error("Thay đổi mật khẩu không thành công");
            return RedirectToAction("Index", "Home", new { Area ="Admin"});
        }

		public IActionResult ProfileAdmin()
		{
			var taikhoanID = HttpContext.Session.GetString("Admin");
            var staffID = HttpContext.Session.GetString("Staff");

			if (taikhoanID != null)
			{
				var admin = _context.TaiKhoans.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
				if (admin != null)
				{
					var models = _context.TaiKhoans
                        .Include(n => n.Nd)
                        .Where(x => x.NdId == admin.NdId)
						.SingleOrDefault();
					return View(models);
				}

			}
            if(staffID != null)
            {

                var staff = _context.TaiKhoans.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(staffID));
                if (staff != null)
                {
                    var models = _context.TaiKhoans
                        .Include(n => n.Nd)
                        .Where(x => x.NdId == staff.NdId)
                        .SingleOrDefault();
                    return View(models);
                }
            }
			return RedirectToAction("Login", "AdminAccount", new { Area = "Admin" });
		}

		public async Task<IActionResult> EditProfileAdmin(int? id)
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
		public async Task<IActionResult> EditProfileAdmin(int id, [Bind("NdId,PnhId,NdHoten,NdHinhanh,NdSdt,NdDiachi,NdGioitinh")] NguoiDung nguoiDung, IFormFile? ifile)
        {
			if (id != nguoiDung.NdId)
			{
				return NotFound();
			}
   //         if(ifile == null)
   //         {
   //             var avt = await _context.NguoiDungs.Where(n => n.NdId == id).Select(n => n.NdHinhanh).FirstOrDefaultAsync();
   //             nguoiDung.NdHinhanh = avt;
			//	ifile = new FormFile(null, 0, 0, "ifile", avt);

			//}
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
                        NguoiDung nguoidungDb = _context.NguoiDungs.AsNoTracking().SingleOrDefault(n => n.NdId == nguoiDung.NdId);
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
				return RedirectToAction(nameof(ProfileAdmin));
			}
			_notyf.Error("Chỉnh sửa thất bại");
			return View(nguoiDung);
		}

        public async Task<IActionResult> GetRole()
        {
            var taikhoanID = HttpContext.Session.GetString("Staff");
			var adminID = HttpContext.Session.GetString("Admin");
			if (taikhoanID != null || adminID != null)
            {
				var admin = _context.TaiKhoans.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(adminID));
				var staff = _context.TaiKhoans.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
                if (staff != null)
                {
                    var info = _context.NguoiDungs.AsNoTracking().SingleOrDefault(n => n.NdId == staff.NdId);
                    return Json(new { status = "success", staff, info });
                }
				if(admin != null)
                {
                    var info = _context.NguoiDungs.AsNoTracking().SingleOrDefault(n => n.NdId == admin.NdId);
                    return Json(new { status = "success", admin, info });
                }
            }
			//            if (adminID != null)
			//            {
			//				var admin = _context.TaiKhoans.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(adminID));
			//.               if (admin != null)
			//				{
			//					var info = _context.NguoiDungs.AsNoTracking().SingleOrDefault(n => n.NdId == admin.NdId);
			//					return Json(new { status = "success", admin, info });
			//				}
			//			}
			return Json(new { status = "error" });

        }

        private bool NguoiDungExists(int id)
		{
			return (_context.TaiKhoans?.Any(e => e.NdId == id)).GetValueOrDefault();
		}
	}
}
