using Microsoft.AspNetCore.Mvc;

namespace BankingApplication.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View(); // This will look for Views/Dashboard/Index.cshtml
        }
    }
}