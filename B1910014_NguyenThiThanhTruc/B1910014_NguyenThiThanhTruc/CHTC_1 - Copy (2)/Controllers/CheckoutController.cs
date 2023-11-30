using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;
using CHTC_1.Extension;
using CHTC_1.Helper;
using CHTC_1.Mail;
using CHTC_1.Models;
using CHTC_1.ModelViews;
using CHTC_1.Notification;
using CHTC_1.VNPayment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X9;
using System.Globalization;
using System.Security.Cryptography;

namespace CHTC_1.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly EmailService _emailService;
        public CheckoutController(Chtc8Context context, INotyfService notyf, IHubContext<NotificationHub> hubContext, EmailService email)
        {
            _context = context;
            _notyf = notyf;
            _hubContext = hubContext;
            _emailService = email;
        }
        public List<CartItem> GioHang
        {
            get
            {
                var gh = HttpContext.Session.Get<List<CartItem>>("GioHang");
                if (gh == default(List<CartItem>))
                {
                    gh = new List<CartItem>();
                }
                return gh;
            }
        }
        public IActionResult Index(string returnUrl = null)
        {
            //Lay gio hang ra de xu ly
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            MuaHangViewModel model = new MuaHangViewModel();
            if (taikhoanID != null)
            {
                var khachhang = _context.TaiKhoans.Include(n => n.Nd).AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.Nd.NdId;
                model.Email = khachhang.TkEmail;
                model.FullName = khachhang.Nd.NdHoten;
                model.Phone = khachhang.Nd.NdSdt;
                model.Address = khachhang.Nd.NdDiachi;
            }
            ViewBag.GioHang = cart;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(MuaHangViewModel muaHang)
        {
            //Lay ra gio hang de xu ly
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            MuaHangViewModel model = new MuaHangViewModel();
            if (taikhoanID != null)
            {
                var khachhang = _context.NguoiDungs.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.NdId;
                model.FullName = khachhang.NdHoten;
                model.Phone = khachhang.NdSdt;
                model.Address = khachhang.NdDiachi;

                khachhang.NdDiachi = muaHang.Address;
                khachhang.NdSdt = muaHang.Phone;

                _context.Update(khachhang);
                _context.SaveChanges();
            }
            try
            {
                if (ModelState.IsValid)
                {
                    //Khoi tao don hang
                    HoaDon donhang = new HoaDon();
                    //donhang.Note = Utilities.StripHTML(model.Note);
                    donhang.PtttId = muaHang.PaymentMethod;
                    donhang.NdId = model.CustomerId;
                    donhang.TthdId = 7;
                    donhang.HdTongtien = Convert.ToInt32(cart.Sum(x => x.Tongtien));
                    donhang.HdNgay = DateTime.Now;
                    donhang.HdThanhtoan = 0;
                    
                    
                    _context.Add(donhang);
                    _context.SaveChanges();
                    //tao danh sach don hang
                   
                    foreach (var item in cart)
                    {
                        var updateSlton = _context.SanPhams.Where(s => s.SpId == item.SanPham.SpId).FirstOrDefault();
                        updateSlton.SpSoluongton = updateSlton.SpSoluongton - item.Soluong;
                        _context.Update(updateSlton);
                        ChiTietHoaDon chitiethoadon = new ChiTietHoaDon();
                        chitiethoadon.HdId = donhang.HdId;
                        chitiethoadon.SpId = item.SanPham.SpId;
                        chitiethoadon.CthdSoluong = item.Soluong;
                        _context.Add(chitiethoadon);
                    }
                    _context.SaveChanges();
                    string orderInfo = donhang.HdId.ToString();
                    if (muaHang.PaymentMethod == 2)
                    {
                        string vnpayUrl = CreateVnpayUrl(donhang.HdTongtien, orderInfo);
                        return Redirect(vnpayUrl);
                    }
                    //clear gio hang
                    HttpContext.Session.Remove("GioHang");
                    var taikhoanId = HttpContext.Session.GetString("CustomerId");
                    var khachhang = _context.NguoiDungs.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanId));
                    // thong bao realtime
                    var thongBao = new ThongBao();
                    thongBao.TbTieude = "Đơn đặt hàng mới";
                    thongBao.TbNoidung = khachhang.NdHoten + " đã đặt một đơn hàng. Xác nhận ngay!";
                    thongBao.TbNguoigui = khachhang.NdId;
                    thongBao.TbThoigian = DateTime.Now;
                    thongBao.TbTrangthai = 0;
                    thongBao.TbNguoinhan = 0;
                    thongBao.TbAvt = khachhang.NdHinhanh;
                    _context.Add(thongBao);
                    await _context.SaveChangesAsync();
                    var message = "Có đơn đặt hàng mới từ: ";
                    await _hubContext.Clients.All.SendAsync("SendMessage",khachhang.NdId,message);
                    //gửi mail
                    var billInfo = _context.HoaDons.FirstOrDefault(b => b.HdId == donhang.HdId);
                    var pttten = "";
                    CultureInfo cultureInfo = new CultureInfo("vi-VN");
                    if (billInfo.PtttId == 1) {
                        pttten = "Thanh toán khi giao hàng";
                    }
                    else
                    {
                        pttten = "Thanh toán qua VNPAY";
                    }
                    string contentEmail = "<html><body>" +
                            "<p>Đặt hàng thành công. Cảm ơn quý khách đã mua hàng tại Doca.</p>" +
                            "<p>Thông tin đơn hàng.</p>" +
                            "<ul>" +
                            "<li>Tài khoản: " + muaHang.Email + "</li>" +
                            "<li>Khách hàng: " + muaHang.FullName + "</li>" +
                            "<li>Số điện thoại: " + muaHang.Phone + "</li>" +
                            "<li>Địa chỉ: " + muaHang.Address + "</li>" +
                            "<li>Phương thức thanh toán: " + pttten + "</li>" +
                            "<li>Ngày đặt: " + billInfo.HdNgay.ToString("dd/MM/yyyy") + "</li>" +
                            "<li>Tổng tiền hóa đơn: " + billInfo.HdTongtien.ToString("C", cultureInfo) + "</li>" +
                            "</ul>" +
                            "<table style='text-align:center; boder: 1px solid #ccc'>" +
                                "<tr>" +
                                    "<th>STT</th>" +
                                    "<th>Sản phẩm</th>" +
                                    "<th>Số lượng</th>" +
                                    "<th>Giá</th>" +
                                    "<th>Thành tiền</th>" +
                                "</tr>";
                    var billdetails = _context.ChiTietHoaDons
                       .Include(s => s.Sp)
                       .AsNoTracking()
                       .Where(x => x.HdId == donhang.HdId)
                       .OrderBy(x => x.SpId)
                       .ToList();
                    for (int i = 0; i < billdetails.Count; i++)
                    {
                        string tenSp = billdetails[i].Sp.SpTensp;
                        int soluong = billdetails[i].CthdSoluong;
                        decimal gia = billdetails[i].Sp.SpGia;
                        decimal thanhtien = billdetails[i].Sp.SpGia * soluong;

                        contentEmail +=
                            "<tr>" +
                                $"<td>{i + 1}</td>" +
                                $"<td>{tenSp}</td>" +
                                $"<td>{soluong}</td>" +
                                $"<td>{gia.ToString("C", cultureInfo)}</td>" +
                                $"<td>{thanhtien.ToString("C", cultureInfo)}</td>" +
                            "</tr>";
                    }
                    contentEmail += "</table></body></html>";
                    await _emailService.SendEmailAsync(muaHang.Email, "Xác nhận mua hàng tại Doca.", contentEmail);
                    //Xuat thong bao
                    _notyf.Success("Đơn hàng đặt thành công");
                   
                    //cap nhat thong tin khach hang
                    return RedirectToAction("Index", "Home");

                }
            }
            catch
            {
                ViewBag.GioHang = cart;
                return View(model);
            }
            
            ViewBag.GioHang = cart;
            return View(model);
        }

        public string CreateVnpayUrl(decimal amount, string orderInfo)
        {
            var storeCode = "BI3XFFKF";
            var secretCode = "NNLBRAMERJPKTNLRUVTFHERSPEMXCXVH";

            VnPayLibrary vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", storeCode);
            vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "210000");
            vnpay.AddRequestData("vnp_ReturnUrl", Url.Action("orderStatus", null, null, Request.Scheme));
            vnpay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());
            vnpay.AddRequestData("vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"));
            string paymentUrl = vnpay.CreateRequestUrl("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html", secretCode);
            return paymentUrl;
        }

        [HttpGet]
        public async Task<IActionResult> OrderStatus(string vnp_Amount, string vnp_BankCode, string vnp_CardType, string vnp_BankTranNo, string vnp_OrderInfo, string vnp_PayDate, string vnp_ResponseCode, string vnp_TmnCode, string vnp_TransactionNo, string vnp_TransactionStatus, string vnp_TxnRef, string vnp_SecureHash)
        {
            
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            string orderInfo = vnp_OrderInfo;
            int hoaDonId = Convert.ToInt32(orderInfo);
            if (vnp_BankCode != "COD")
            {
                var secretCode = "NNLBRAMERJPKTNLRUVTFHERSPEMXCXVH";
                VnPayLibrary vnpay = new VnPayLibrary();
                vnpay.AddResponseData("vnp_Amount", vnp_Amount);
                vnpay.AddResponseData("vnp_BankCode", vnp_BankCode);
                vnpay.AddResponseData("vnp_BankTranNo", vnp_BankTranNo);
                vnpay.AddResponseData("vnp_CardType", vnp_CardType);
                vnpay.AddResponseData("vnp_OrderInfo", vnp_OrderInfo);
                vnpay.AddResponseData("vnp_PayDate", vnp_PayDate);
                vnpay.AddResponseData("vnp_ResponseCode", vnp_ResponseCode);
                vnpay.AddResponseData("vnp_TmnCode", vnp_TmnCode);
                vnpay.AddResponseData("vnp_TransactionNo", vnp_TransactionNo);
                vnpay.AddResponseData("vnp_TransactionStatus", vnp_TransactionStatus);
                vnpay.AddResponseData("vnp_TxnRef", vnp_TxnRef);
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, secretCode);
                if (!checkSignature)
                {
                    _notyf.Success("Thanh toán thất bại");

                    //cap nhat thong tin khach hang
                    return RedirectToAction("Index", "Home");
                   

                }
            }
            try
            {
                HoaDon hoaDon = _context.HoaDons.FirstOrDefault(x => x.HdId == hoaDonId && x.NdId.ToString() == taikhoanID);
               
                if (vnp_ResponseCode != "00")
                {
                    _notyf.Information("Thanh toán qua cổng VNPay thất bại");

                    //cap nhat thong tin khach hang
                    return RedirectToAction("Index", "Home");
                    
                }
                if (vnp_BankCode != "COD")
                {
                    hoaDon.HdThanhtoan = 1;
                    await _context.SaveChangesAsync();
                    var khachhang = _context.NguoiDungs.AsNoTracking().SingleOrDefault(x => x.NdId == Convert.ToInt32(taikhoanID));
                    var accountInfo = _context.TaiKhoans.AsNoTracking().SingleOrDefault(a => a.NdId == Convert.ToInt32(taikhoanID));
                    //gửi mail
                    var billInfo = _context.HoaDons.AsNoTracking().SingleOrDefault(b => b.HdId == hoaDon.HdId);
                    var pttten = "";
                    CultureInfo cultureInfo = new CultureInfo("vi-VN");
                    string contentEmail = "<html><body>" +
                            "<p>Đặt hàng thành công. Cảm ơn quý khách đã mua hàng tại Doca.</p>" +
                            "<p>Thông tin đơn hàng.</p>" +
                            "<ul>" +
                            "<li>Tài khoản: " + accountInfo.TkEmail + "</li>" +
                            "<li>Khách hàng: " + khachhang.NdHoten + "</li>" +
                            "<li>Số điện thoại: " + khachhang.NdSdt + "</li>" +
                            "<li>Địa chỉ: " + khachhang.NdDiachi + "</li>" +
                            "<li>Phương thức thanh toán: " + "Thanh toán qua VNPAY" + "</li>" +
                            "<li>Ngày đặt: " + billInfo.HdNgay.ToString("dd/MM/yyyy") + "</li>" +
                            "<li>Tổng tiền hóa đơn: " + billInfo.HdTongtien.ToString("C", cultureInfo) + "</li>" +
                            "</ul>" +
                            "<table style='text-align:center; boder: 1px solid #ccc'>" +
                                "<tr>" +
                                    "<th>STT</th>" +
                                    "<th>Sản phẩm</th>" +
                                    "<th>Số lượng</th>" +
                                    "<th>Giá</th>" +
                                    "<th>Thành tiền</th>" +
                                "</tr>";
                    var billdetails = _context.ChiTietHoaDons
                       .Include(s => s.Sp)
                       .AsNoTracking()
                       .Where(x => x.HdId == hoaDon.HdId)
                       .OrderBy(x => x.SpId)
                       .ToList();
                    for (int i = 0; i < billdetails.Count; i++)
                    {
                        string tenSp = billdetails[i].Sp.SpTensp;
                        int soluong = billdetails[i].CthdSoluong;
                        decimal gia = billdetails[i].Sp.SpGia;
                        decimal thanhtien = billdetails[i].Sp.SpGia * soluong;

                        contentEmail +=
                            "<tr>" +
                                $"<td>{i + 1}</td>" +
                                $"<td>{tenSp}</td>" +
                                $"<td>{soluong}</td>" +
                                $"<td>{gia.ToString("C", cultureInfo)}</td>" +
                                $"<td>{thanhtien.ToString("C", cultureInfo)}</td>" +
                            "</tr>";
                    }
                    contentEmail += "</table></body></html>";
                    await _emailService.SendEmailAsync(accountInfo.TkEmail, "Xác nhận mua hàng tại Doca.", contentEmail);
                    //realtime
                    
                    _notyf.Success("Đã thanh toán thành công");
                    HttpContext.Session.Remove("GioHang");
                    var thongBao = new ThongBao();
                    thongBao.TbTieude = "Đơn đặt hàng mới";
                    thongBao.TbNoidung = khachhang.NdHoten + " đã đặt một đơn hàng. Xác nhận ngay!";
                    thongBao.TbNguoigui = khachhang.NdId;
                    thongBao.TbThoigian = DateTime.Now;
                    thongBao.TbTrangthai = 0;
                    thongBao.TbNguoinhan = 0;
                    thongBao.TbAvt = khachhang.NdHinhanh;
                    _context.Add(thongBao);
                    await _context.SaveChangesAsync();
                    var message = "Có đơn đặt hàng mới từ: ";
                    await _hubContext.Clients.All.SendAsync("SendMessage", khachhang.NdId, message);
                    //cap nhat thong tin khach hang
                    return RedirectToAction("Index", "Home");

                }
                HttpContext.Session.Remove("GioHang");
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                
                _notyf.Success("Lỗi");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
