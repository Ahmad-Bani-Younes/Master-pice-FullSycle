using Master_pice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master_pice.Areas.Admin.Controllers
{

    [Area("Admin")]

    public class ReportsController : Controller
    {

        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {

            ViewBag.Users = _context.Users.Count();
            ViewBag.Products = _context.Laptops.Count() + _context.PCs.Count() + _context.PCParts.Count();
            ViewBag.Completed = _context.Orders.Count(o => o.OrderStatus == "Delivered");
            ViewBag.Cancelled = _context.Orders.Count(o => o.OrderStatus == "Cancelled");
            ViewBag.Processing = _context.Orders.Count(o => o.OrderStatus == "Processing");

            return View();
        }

        public IActionResult DownloadReport()
        {
            var content = "This is a sample report. You can replace this with actual data.";
            return File(System.Text.Encoding.UTF8.GetBytes(content), "application/pdf", "report.pdf");
        }
    }
}
