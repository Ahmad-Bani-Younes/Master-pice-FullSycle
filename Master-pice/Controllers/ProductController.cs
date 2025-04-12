using Master_pice.Models;
using Master_pice.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public class ProductController : Controller
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

   



    public IActionResult Details(int id, string type)
    {
        object product = null;

        if (id != null)
        {
            switch (type)
            {
                case "pc":
                    product = _context.PCs.FirstOrDefault(p => p.PCID == id);
                    break;
                case "laptop":
                    product = _context.Laptops.FirstOrDefault(p => p.LaptopID == id);
                    break;
                case "pcpart":
                    product = _context.PCParts.FirstOrDefault(p => p.PartID == id);
                    break;
            }
        }


        if (product == null)
            return NotFound();

        return View(product);
    }




    public IActionResult Cart()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        List<CartItemViewModel> cartItems = new();

        if (userId != null)
        {
            var carts = _context.Cart.Where(c => c.UserID == userId).ToList();

            foreach (var c in carts)
            {
                var item = new CartItemViewModel
                {
                    ProductId = c.ProductId,
                    Type = c.Type,
                    Quantity = c.Quantity
                };

                switch (c.Type.ToLower())
                {
                    case "pc":
                        var pc = _context.PCs.FirstOrDefault(p => p.PCID == c.ProductId);
                        if (pc != null)
                        {
                            item.Name = pc.Brand + " " + pc.Processor;
                            item.ImageURL = pc.ImageURL;
                            item.Price = pc.Price;
                        }
                        break;

                    case "laptop":
                        var laptop = _context.Laptops.FirstOrDefault(l => l.LaptopID == c.ProductId);
                        if (laptop != null)
                        {
                            item.Name = laptop.Brand + " " + laptop.Model;
                            item.ImageURL = laptop.ImageURL;
                            item.Price = laptop.Price;
                        }
                        break;

                    case "part":
                        var part = _context.PCParts.FirstOrDefault(p => p.PartID == c.ProductId);
                        if (part != null)
                        {
                            item.Name = part.Model;
                            item.ImageURL = part.ImageURL;
                            item.Price = part.Price;
                        }
                        break;
                }

                cartItems.Add(item);
            }

            ViewBag.UserId = userId; // لترسله للفيو
        }
        else
        {
            // للمستخدم غير المسجل - من localStorage
            string cookieData = Request.Cookies["questCart"];
            if (!string.IsNullOrEmpty(cookieData))
            {
                cartItems = JsonConvert.DeserializeObject<List<CartItemViewModel>>(cookieData);
            }

            ViewBag.UserId = 0;
        }

        return View(cartItems);
    }

    [HttpPost]
    public IActionResult AddToCart(CartItemViewModel model)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Authintication");

        var existing = _context.Cart.FirstOrDefault(c =>
            c.UserID == userId &&
            c.ProductId == model.ProductId &&
            c.Type == model.Type);

        if (existing != null)
        {
            existing.Quantity += model.Quantity;
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


    [HttpPost]
    public IActionResult UpdateQuantity(int ProductId, string Type, int Quantity)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId != null)
        {
            var item = _context.Cart.FirstOrDefault(c => c.UserID == userId && c.ProductId == ProductId && c.Type == Type);
            if (item != null)
            {
                item.Quantity = Quantity;
                _context.SaveChanges();
            }
        }
        return RedirectToAction("Cart");
    }

    [HttpPost]
    public IActionResult RemoveFromCart(int ProductId, string Type)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId != null)
        {
            var item = _context.Cart.FirstOrDefault(c => c.UserID == userId && c.ProductId == ProductId && c.Type == Type);
            if (item != null)
            {
                _context.Cart.Remove(item);
                _context.SaveChanges();
            }
        }
        return RedirectToAction("Cart");
    }


    [HttpPost]
    public IActionResult ClearCart()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId != null)
        {
            var userCartItems = _context.Cart.Where(c => c.UserID == userId);
            _context.Cart.RemoveRange(userCartItems);
            _context.SaveChanges();
        }

        return RedirectToAction("Cart");
    }


    [HttpGet]
    public JsonResult GetCartCount()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        int count = 0;

        if (userId != null)
        {
            count = _context.Cart
                            .Where(c => c.UserID == userId)
                            .Sum(c => c.Quantity);
        }

        return Json(new { count });
    }





}
