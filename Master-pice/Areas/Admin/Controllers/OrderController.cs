using Master_pice.Models;
using Master_pice.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master_pice.Areas.Admin.Controllers
{

    [Area("Admin")]

    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderID)
                .ToList();

            var payments = _context.Payments.ToList();

            var model = orders.Select(o => new OrderWithPaymentViewModel
            {
                Order = o,
                PaymentMethod = payments.FirstOrDefault(p => p.OrderID == o.OrderID)?.PaymentMethod ?? "-"
            }).ToList();

            return View(model);
        }


        // POST: Admin/Order/UpdateStatus/5
        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return NotFound();

            order.OrderStatus = status;
            _context.SaveChanges();

            return RedirectToAction("PendingOrders");
        }


        [HttpGet]
        public IActionResult PendingOrders()
        {
            var pendingOrders = _context.Orders
                .Where(o => o.OrderStatus == "Pending" || o.OrderStatus == "Processing")
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(pendingOrders);
        }




        [HttpGet]
        public IActionResult DetailsOrderAdmin(int id)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
                return NotFound();

            // جلب معلومات الدفع المرتبطة بالطلب
            var payment = _context.Payments
                .Where(p => p.OrderID == id)
                .Select(p => new
                {
                    p.PaymentMethod,
                    p.ReceiptImage,
                    p.Amount,
                    p.IsPaid,
                    p.PaymentDate
                })
                .FirstOrDefault();

            // إرسالها للـ View عبر ViewBag
            ViewBag.PaymentMethod = payment?.PaymentMethod;
            ViewBag.ReceiptImage = payment?.ReceiptImage;
            ViewBag.IsPaid = payment?.IsPaid;
            ViewBag.Amount = payment?.Amount;
            ViewBag.PaymentDate = payment?.PaymentDate;

            return View(order);
        }


        
        public int GetPendingOrdersCount()
        {
            return _context.Orders.Count(o => o.OrderStatus == "Pending" || o.OrderStatus == "Processing");
        }




    }
}

