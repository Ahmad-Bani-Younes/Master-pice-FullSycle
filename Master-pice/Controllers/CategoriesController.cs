using Master_pice.Models;
using Master_pice.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Master_pice.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        //public IActionResult Index(String type)
        //{
        //    ViewBag.Type = type;
        //    return View("Products");
        //}

        public IActionResult Products(String? type)
        {
            var laptops = _context.Laptops.Select(p => new ProductViewModel
            {
                ID = p.LaptopID,
                Name = p.Model,
                Description = p.Description,
                Price = p.Price,
                ImageURL = p.ImageURL,
                Type = "Laptop"
            });

            var pcs = _context.PCs.Select(p => new ProductViewModel
            {
                ID = p.PCID,
                Name = p.Brand + " PC",
                Description = p.Description,
                Price = p.Price,
                ImageURL = p.ImageURL,
                Type = "PC"
            });

            var parts = _context.PCParts.Select(p => new ProductViewModel
            {
                ID = p.PartID,
                Name = p.Model,
                Description = p.Compatibility,
                Price = p.Price,
                ImageURL = p.ImageURL,
                Type = "PCPart"
            });

            var allProducts = laptops.Concat(pcs).Concat(parts).ToList();
            ViewBag.Type = type;
            return View(allProducts);
        }


        // ✅ للأجهزة المكتبية
        public IActionResult PCs(String? type)
        {
            var products = _context.PCs.Select(p => new ProductViewModel
            {
                ID = p.PCID,
                Name = p.Brand + " " + p.Processor,
                Description = p.Description,
                Price = p.Price,
                ImageURL = p.ImageURL,
                Type = "pc"
            }).ToList();

            ViewBag.Type = type;
            return View("ProductList", products); // استخدم نفس View
        }

        // ✅ للابتوبات
        public IActionResult Laptops(String? type)
        {
            var products = _context.Laptops.Select(p => new ProductViewModel
            {
                ID = p.LaptopID,
                Name = p.Brand + " " + p.Model,
                Description = p.Description,
                Price = p.Price,
                ImageURL = p.ImageURL,
                Type = "laptop"
            }).ToList();

            ViewBag.Type = type;
            return View("ProductList", products); // نفس View مشترك
        }

        // ✅ لقطع الـ PC
        public IActionResult PCParts(String? type)
        {
            var products = _context.PCParts.Select(p => new ProductViewModel
            {
                ID = p.PartID,
                Name = p.Model,
                Description = p.Compatibility,
                Price = p.Price,
                ImageURL = p.ImageURL,
                Type = "pcpart"
            }).ToList();
            ViewBag.Type = type;
            return View("ProductList", products); // نفس View مشترك
        }


        public IActionResult ProductList()
        {
           return View();
        }



        [HttpPost]
        public IActionResult AddToCart(int ProductId, string Type, int Quantity)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Authintication");

            var existing = _context.Cart.FirstOrDefault(c =>
                c.UserID == userId &&
                c.ProductId == ProductId &&
                c.Type == Type);

            if (existing != null)
            {
                existing.Quantity += Quantity;
            }
            else
            {
                _context.Cart.Add(new Cart
                {
                    UserID = userId.Value,
                    ProductId = ProductId,
                    Type = Type,
                    Quantity = Quantity
                });
            }

            _context.SaveChanges();

            return Type.ToLower() switch
            {
                "pc" => RedirectToAction("PCs", new { type = "PC" }),
                "laptop" => RedirectToAction("Laptops", new { type = "Laptop" }),
                "pcpart" => RedirectToAction("PCParts", new { type = "PCPart" }),
                _ => RedirectToAction("Products", new { type = "All" })
            };
        }




    }
}
