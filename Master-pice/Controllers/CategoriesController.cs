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

        public IActionResult Products(string? type, List<string>? prices, int page = 1)
        {
            int pageSize = 12;

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

            var uniqueCategories = allProducts
                .Select(p => p.Name.Split(" ")[0])
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            ViewBag.Categories = uniqueCategories;

            if (!string.IsNullOrEmpty(type))
            {
                allProducts = allProducts
                    .Where(p => p.Name.StartsWith(type, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // فلترة حسب السعر
            var priceRanges = new List<(decimal Min, decimal Max)>();
            if (prices != null && prices.Any())
            {
                foreach (var range in prices)
                {
                    var rangeParts = range.Replace("JD", "").Split('-');
                    if (rangeParts.Length == 2 &&
                        decimal.TryParse(rangeParts[0].Trim(), out var min) &&
                        decimal.TryParse(rangeParts[1].Trim(), out var max))
                    {
                        priceRanges.Add((min, max));
                    }
                }


                allProducts = allProducts
                    .Where(p => priceRanges.Any(r => p.Price >= r.Min && p.Price <= r.Max))
                    .ToList();
            }

            int totalProducts = allProducts.Count;
            var pagedProducts = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Type = type;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentPage = page;

            return View(pagedProducts);
        }



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
            return View("ProductList", products); 
        }

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
            return View("ProductList", products); 
        }

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
            return View("ProductList", products); 
        }


        public IActionResult ProductList()
        {
           return View();
        }



        [HttpPost]
        public IActionResult AddToCart(CartItemViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                var cookieData = Request.Cookies["Cart"];
                List<CartItemViewModel> cart = string.IsNullOrEmpty(cookieData)
                    ? new List<CartItemViewModel>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(cookieData) ?? new List<CartItemViewModel>();

                var existing = cart.FirstOrDefault(c => c.ProductId == model.ProductId && c.Type.ToLower() == model.Type.ToLower());
                if (existing != null)
                    existing.Quantity += model.Quantity;
                else
                    cart.Add(model);

                var options = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(3),
                    IsEssential = true,
                    HttpOnly = false
                };

                Response.Cookies.Append("Cart", System.Text.Json.JsonSerializer.Serialize(cart), options);

                return RedirectToAction("Products", "Categories");
            }

            var dbCart = _context.Cart.FirstOrDefault(c =>
                c.UserID == userId &&
                c.ProductId == model.ProductId &&
                c.Type.ToLower() == model.Type.ToLower());

            if (dbCart != null)
            {
                dbCart.Quantity += model.Quantity;
            }
            else
            {
                _context.Cart.Add(new Cart
                {
                    UserID = userId.Value,
                    ProductId = model.ProductId,
                    Type = model.Type,
                    Quantity = model.Quantity
                });
            }

            _context.SaveChanges();
            return RedirectToAction("Products", "Categories");
        }




    }
}
