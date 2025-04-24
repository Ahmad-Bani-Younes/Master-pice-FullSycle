using Master_pice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master_pice.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var usersCount = _context.Users.Count();

            var laptopsCount = _context.Laptops.Count();
            var pcsCount = _context.PCs.Count();
            var partsCount = _context.PCParts.Count();
            var totalProducts = laptopsCount + pcsCount + partsCount;

            var ordersCompleted = _context.Orders.Count(o => o.OrderStatus == "Delivered");
            var ordersCancelled = _context.Orders.Count(o => o.OrderStatus == "Cancelled");
            var ordersProcessing = _context.Orders.Count(o => o.OrderStatus == "Processing");

            ViewBag.Users = usersCount;
            ViewBag.Products = totalProducts;
            ViewBag.Completed = ordersCompleted;
            ViewBag.Cancelled = ordersCancelled;
            ViewBag.Processing = ordersProcessing;

            return View();
        }
        public IActionResult AddProduct()
        {
            return View();
        }

       


    }
}
