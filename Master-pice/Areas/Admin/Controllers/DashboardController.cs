using Master_pice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            var usersCount = await _context.Users.CountAsync();
            var laptopsCount = await _context.Laptops.CountAsync();
            var pcsCount = await _context.PCs.CountAsync();
            var partsCount = await _context.PCParts.CountAsync();
            var totalProducts = laptopsCount + pcsCount + partsCount;

            var ordersCompleted = await _context.Orders.CountAsync(o => o.OrderStatus == "Delivered");
            var ordersCancelled = await _context.Orders.CountAsync(o => o.OrderStatus == "Cancelled");
            var ordersProcessing = await _context.Orders.CountAsync(o => o.OrderStatus == "Processing");

            ViewBag.Users = usersCount;
            ViewBag.Products = totalProducts;
            ViewBag.Completed = ordersCompleted;
            ViewBag.Cancelled = ordersCancelled;
            ViewBag.Processing = ordersProcessing;

            // ✅ جلب آخر الأنشطة بشكل ديناميكي
            var recentActivities = new List<string>();

            var lastUser = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => $"✅ New user \"{u.FullName}\" registered.")
                .FirstOrDefaultAsync();

            var lastLaptop = await _context.Laptops
                .OrderByDescending(l => l.LaptopID)
                .Select(l => $"💻 New laptop \"{l.Brand} {l.Model}\" added to inventory.")
                .FirstOrDefaultAsync();

            var lastPC = await _context.PCs
                .OrderByDescending(p => p.PCID)
                .Select(p => $"🖥️ New PC \"{p.Brand}\" added to inventory.")
                .FirstOrDefaultAsync();

            var lastPart = await _context.PCParts
                .OrderByDescending(pp => pp.PartID)
                .Select(pp => $"🛠️ New PC Part \"{pp.Category}\" added to inventory.")
                .FirstOrDefaultAsync();

            var lastOrder = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => $"🚚 Order #{o.OrderID} placed by User {o.UserID}.")
                .FirstOrDefaultAsync();

            if (lastUser != null) recentActivities.Add(lastUser);
            if (lastLaptop != null) recentActivities.Add(lastLaptop);
            if (lastPC != null) recentActivities.Add(lastPC);
            if (lastPart != null) recentActivities.Add(lastPart);
            if (lastOrder != null) recentActivities.Add(lastOrder);

            ViewBag.RecentActivities = recentActivities;

            return View();
        }

        public IActionResult AddProduct()
        {
            return View();
        }
    }
}
