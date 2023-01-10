using BookShopTask.DAL;
using BookShopTask.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShopTask.Controllers
{
    public class HomeController : Controller
    {
        AppDbContext _context { get; }
        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            HomeVM home = new HomeVM { Products = _context.Products.Where(p=>p.IsDeleted == false).Include(p=>p.ProductImages) };
            return View(home);
        }
    }
}
