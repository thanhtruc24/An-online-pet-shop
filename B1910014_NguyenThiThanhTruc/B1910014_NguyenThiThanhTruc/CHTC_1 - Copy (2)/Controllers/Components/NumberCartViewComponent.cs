using CHTC_1.Extension;
using CHTC_1.ModelViews;
using Microsoft.AspNetCore.Mvc;

namespace CHTC_1.Controllers.Component
{
    public class NumberCartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            return View(cart);
        }
    }
}
