using Microsoft.AspNetCore.Mvc;

namespace CHTC_1.Controllers
{
    public class AjaxContentController : Controller
    {
        public IActionResult NumberCart()
        {
            return ViewComponent("NumberCart");
        }
    }
}
