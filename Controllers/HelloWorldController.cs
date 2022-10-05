using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace blog.Controllers
{
    public class HelloWorldController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
