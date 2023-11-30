using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;
using CHTC_1.Helper;
using CHTC_1.Mail;
using CHTC_1.Models;
using CHTC_1.ModelViews;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Security.Principal;

namespace CHTC_1.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _environment;
        private readonly EmailService _emailService;

        public AccountsController(Chtc8Context context, INotyfService notyf, IWebHostEnvironment environment, EmailService email)
        {
            _context = context;
            _notyf = notyf;
            _environment = environment;
            _emailService = email;
        }

        public IActionResult Index()
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                var khachhang = _context.NguoiDungs.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
                if (khachhang != null)
                {
                    //var lsDonHang = _context.HoaDons
                    //    .Include(x => x.ChiTietHoaDons)
                    //    .AsNoTracking()
                    //    .Where(x => x.NdId == khachhang.NdId)
                    //    .OrderByDescending(x => x.HdNgay)
                    //    .ToList();
                    //ViewBag.DonHang = lsDonHang;
                    return View(khachhang);
                }

            }
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidateEmail(string Email)
        {
            try
            {
                var khachhang = _context.TaiKhoans.AsNoTracking().SingleOrDefault(x => x.TkEmail.ToLower() == Email.ToLower());
                if (khachhang != null)
                    return Json(data: "Email : " + Email + " đã được sử dụng");
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel taikhoan)
        {
            try
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

                        NguoiDung nguoidung = new NguoiDung
                        {
                            NdHoten = taikhoan.FullName,
                            NdDiachi = taikhoan.Address,
                            NdSdt = taikhoan.Phone,
                            NdHinhanh = "avt1.jpg"
                        };
                        _context.Add(nguoidung);
                        await _context.SaveChangesAsync();
                        //Lưu Session MaKh
                        HttpContext.Session.SetString("CustomerId", nguoidung.NdId.ToString());
                        var taikhoanID = HttpContext.Session.GetString("CustomerId");
                        var nguoiDung = _context.NguoiDungs.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
                        khachhang.NdId = nguoiDung.NdId;
                        khachhang.VtId = 3;
                        _context.Add(khachhang);
                        await _context.SaveChangesAsync();
                        //Identity
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Email,khachhang.TkEmail),
                            new Claim("CustomerId", khachhang.NdId.ToString())
                        };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        //await _emailService.SendEmailAsync(khachhang.TkEmail, "Đăng ký tài khoản", "Xin chào");
                        await _emailService.SendEmailAsync(taikhoan.Email, "Đăng ký tài khoản thành công.",
                            "<html><body>" +
                            
                            "<p>Chào mừng bạn đã trở thành khách hàng thân thiết của DoCa shop.</p>" +
                            
                            "<ul>" +
                            "<li>Tài khoản: " + taikhoan.Email + "</li>" +
                            "<li>Mật khẩu: " + taikhoan.Password + "</li>" +
                            "</ul>" +
                            //"<img src='' alt='Mô tả hình ảnh'>" +
                            "</body></html>");
                        _notyf.Success("Đăng ký thành công");
                        return RedirectToAction("Index", "Home");
                    }
                    catch
                    {
                        _notyf.Error("Đăng ký thất bại");
                        return RedirectToAction("Register", "Accounts");
                    }
                }
                else
                {
                    return View(taikhoan);
                }
            }
            catch
            {
                return View(taikhoan);
            }
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]

        public async Task<IActionResult> Login(LoginViewModel customer, string returnUrl = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(customer.Email);
                    if (!isEmail) return View(customer);

                    var khachhang = _context.TaiKhoans.AsNoTracking().SingleOrDefault(x => x.TkEmail.Trim() == customer.Email);

                    if (khachhang == null)
                    {
                        _notyf.Error("Tài khoản không tồn tại");
                        return RedirectToAction("Register");
                    }

                    string pass = customer.Password;
                    if (khachhang.TkMatkhau != pass)
                    {
                        _notyf.Error("Thông tin đăng nhập chưa chính xác");
                        return View(customer);
                    }
                    //kiem tra xem account co bi disable hay khong

                    //Luu Session MaKh
                    HttpContext.Session.SetString("CustomerId", khachhang.NdId.ToString());
                    var taikhoanID = HttpContext.Session.GetString("CustomerId");

                    //Identity
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, khachhang.TkEmail),
                        new Claim("CustomerId", khachhang.NdId.ToString())
                    };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    _notyf.Success("Đăng nhập thành công");
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return Redirect(returnUrl);
                    }
                }
            }
            catch
            {
                return RedirectToAction("Register", "Accounts");
            }
            return View(customer);
        }


        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("CustomerId");
            _notyf.Success("Hẹn gặp lại");
            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> GetCustomer()
        {
			//HttpContext.Session.SetString("CustomerId", nguoidung.NdId.ToString());
			var taikhoanID = HttpContext.Session.GetString("CustomerId");
			var nguoiDung = _context.NguoiDungs.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
			return Json(new { status = "success", nguoiDung });

		}

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (taikhoanID == null)
                {
                    return RedirectToAction("Login", "Accounts");
                }
                if (ModelState.IsValid)
                {
                    var taikhoan = _context.TaiKhoans.Find(Convert.ToInt32(taikhoanID));
                    if (taikhoan == null) return RedirectToAction("Login", "Accounts");
                    var pass = model.PasswordNow.Trim();
                    {
                        string passnew = model.Password.Trim();
                        taikhoan.TkMatkhau = passnew;
                        _context.Update(taikhoan);
                        _context.SaveChanges();
                        _notyf.Success("Đổi mật khẩu thành công");
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch
            {
                _notyf.Error("Thay đổi mật khẩu không thành công");
                return RedirectToAction("Index", "Home");
            }
            _notyf.Error("Thay đổi mật khẩu không thành công");
            return RedirectToAction("Index", "Home");
        }

        //login bằng gg
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GoogleLogin(string provider)
        {
            var redirectUrl = Url.Action(nameof(GoogleLoginCallback), "Accounts");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLoginCallback()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = HttpContext.User;
                var nameClaim = claimsPrincipal.FindFirst(ClaimTypes.Name).Value;
                var emailClaim = claimsPrincipal.FindFirst(ClaimTypes.Email).Value;

                if (!result.Succeeded)
                {
                    // Xử lý khi đăng nhập thất bại
                    _notyf.Error("Có lỗi xảy ra");
                    return RedirectToAction("Login");
                }
                var khachhang = _context.TaiKhoans.Include(n => n.Nd).AsNoTracking().SingleOrDefault(x => x.TkEmail.Trim() == emailClaim);
                if (khachhang == null)
                {
                    TaiKhoan taikhoan = new TaiKhoan
                    {
                        TkEmail = emailClaim
                    };
                    //tạo thông tin người dùng mới
                    var nguoidung = new NguoiDung
                    {
                        NdHoten = nameClaim,
                        NdHinhanh = "avt1.jpg"
                    };
                    _context.Add(nguoidung);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("CustomerId", nguoidung.NdId.ToString());
                    var taikhoanID = HttpContext.Session.GetString("CustomerId");
                    var nguoiDung = _context.NguoiDungs.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
                    taikhoan.NdId = nguoiDung.NdId;
                    taikhoan.VtId = 3;
                    _context.Add(taikhoan);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    HttpContext.Session.SetString("CustomerId", khachhang.NdId.ToString());
                }

                // Xử lý khi đăng nhập thành công
                // Có thể thêm mã code để đăng ký hoặc cập nhật thông tin người dùng tại đây
                _notyf.Success("Đăng nhập thành công");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _notyf.Error("Có lỗi xảy ra");
                return RedirectToAction("Login");
            }
        }

        //login bằng fb

        [HttpGet]
        [AllowAnonymous]
        public IActionResult FacebookLogin(string provider)
        {
            var redirectUrl = Url.Action(nameof(FacebookLoginCallback), "Accounts");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        // Xử lý sau khi đăng nhập thành công
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> FacebookLoginCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = HttpContext.User;
            var nameClaim = claimsPrincipal.FindFirst(ClaimTypes.Name).Value;
            var emailClaim = claimsPrincipal.FindFirst(ClaimTypes.Email).Value;
            if (!result.Succeeded)
            {
                // Xử lý khi đăng nhập thất bại
                _notyf.Error("Có lỗi xảy ra");
                return RedirectToAction("Login");
            }

            // Xử lý khi đăng nhập thành công
            // Có thể thêm mã code để đăng ký hoặc cập nhật thông tin người dùng tại đây

            _notyf.Success("Đăng nhập thành công");
            return RedirectToAction("Index", "Home");
        }


    }
}
