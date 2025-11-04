using Microsoft.AspNetCore.Mvc;

namespace SeamlessGo
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
