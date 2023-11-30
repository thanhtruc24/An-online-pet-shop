using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;
using CHTC_1.Extension;
using CHTC_1.Models;
using CHTC_1.ModelViews;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CHTC_1.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly Chtc8Context _context;
        private readonly INotyfService _notyf;
        public ShoppingCartController(Chtc8Context context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
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

        public IActionResult AddToCart(int productID, int? amount)
        {
            List<CartItem> cart = GioHang;

            try
            {
                //Them san pham vao gio hang
                CartItem item = cart.SingleOrDefault(p => p.SanPham.SpId == productID);
                if (item != null) // da co => cap nhat so luong
                {
                    item.Soluong = item.Soluong + amount.Value;
                    //luu lai session
                    HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                }
                else
                {
                    SanPham sp = _context.SanPhams.SingleOrDefault(p => p.SpId == productID);
                    item = new CartItem
                    {
                        Soluong = amount.HasValue ? amount.Value : 1,
                        SanPham = sp
                    };
                    cart.Add(item);//Them vao gio
                }

                //Luu lai Session
                HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
        [HttpPost]
        public IActionResult UpdateCart(int productID, int? amount)
        {
            //Lay gio hang ra de xu ly
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            try
            {
                if (cart != null)
                {
                    CartItem item = cart.SingleOrDefault(p => p.SanPham.SpId == productID);
                    if (item != null && amount.HasValue) // da co -> cap nhat so luong
                    {
                        item.Soluong = amount.Value;
                    }
                    //Luu lai session
                    HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                }
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public ActionResult Remove(int productID)
        {

            try
            {
                List<CartItem> gioHang = GioHang;
                CartItem item = gioHang.SingleOrDefault(p => p.SanPham.SpId == productID);
                if (item != null)
                {
                    gioHang.Remove(item);
                }
                //luu lai session
                HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);
                _notyf.Success("Xoa san pham thanh cong");
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        public IActionResult Index()
        {
            return View(GioHang);
        }

    }
}
