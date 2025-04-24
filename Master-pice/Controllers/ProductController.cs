using Master_pice.Models;
using Master_pice.ViewModel;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;

public class ProductController : Controller
{
    private readonly AppDbContext _context;
    private readonly IAntiforgery _antiforgery;


    public ProductController(AppDbContext context, IAntiforgery antiforgery)
    {
        _context = context;
        _antiforgery = antiforgery;
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


    //public IActionResult Cart()
    //{
    //    int? userId = HttpContext.Session.GetInt32("UserId");
    //    List<CartItemViewModel> cartItems = new();

    //    if (userId != null)
    //    {
    //        var carts = _context.Cart.Where(c => c.UserID == userId).ToList();

    //        foreach (var c in carts)
    //        {
    //            var item = new CartItemViewModel
    //            {
    //                ProductId = c.ProductId,
    //                Type = c.Type,
    //                Quantity = c.Quantity
    //            };

    //            switch (c.Type.ToLower())
    //            {
    //                case "pc":
    //                    var pc = _context.PCs.FirstOrDefault(p => p.PCID == c.ProductId);
    //                    if (pc != null)
    //                    {
    //                        item.Name = pc.Brand + " " + pc.Processor;
    //                        item.ImageURL = pc.ImageURL;
    //                        item.Price = pc.Price;
    //                    }
    //                    break;

    //                case "laptop":
    //                    var laptop = _context.Laptops.FirstOrDefault(l => l.LaptopID == c.ProductId);
    //                    if (laptop != null)
    //                    {
    //                        item.Name = laptop.Brand + " " + laptop.Model;
    //                        item.ImageURL = laptop.ImageURL;
    //                        item.Price = laptop.Price;
    //                    }
    //                    break;

    //                case "part":
    //                    var part = _context.PCParts.FirstOrDefault(p => p.PartID == c.ProductId);
    //                    if (part != null)
    //                    {
    //                        item.Name = part.Model;
    //                        item.ImageURL = part.ImageURL;
    //                        item.Price = part.Price;
    //                    }
    //                    break;
    //            }

    //            cartItems.Add(item);
    //        }

    //        ViewBag.UserId = userId; // لترسله للفيو
    //    }
    //    else
    //    {
    //        // للمستخدم غير المسجل - من localStorage
    //        string cookieData = Request.Cookies["questCart"];
    //        if (!string.IsNullOrEmpty(cookieData))
    //        {
    //            cartItems = JsonConvert.DeserializeObject<List<CartItemViewModel>>(cookieData);
    //        }

    //        ViewBag.UserId = 0;
    //    }

    //    return View(cartItems);
    //}

    //[HttpPost]
    //public IActionResult AddToCart(CartItemViewModel model)
    //{
    //    int? userId = HttpContext.Session.GetInt32("UserId");
    //    if (userId == null)
    //        return RedirectToAction("Login", "Authintication");

    //    var existing = _context.Cart.FirstOrDefault(c =>
    //        c.UserID == userId &&
    //        c.ProductId == model.ProductId &&
    //        c.Type == model.Type);

    //    if (existing != null)
    //    {
    //        existing.Quantity += model.Quantity;
    //    }
    //    else
    //    {
    //        _context.Cart.Add(new Cart
    //        {
    //            UserID = userId.Value,
    //            ProductId = model.ProductId,
    //            Type = model.Type,
    //            Quantity = model.Quantity
    //        });
    //    }

    //    _context.SaveChanges();
    //    return RedirectToAction("Products", "Categories");
    //}

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

            ViewBag.UserId = userId; // المستخدم مسجل
        }
        else
        {
            // المستخدم غير مسجل - سلة الكوكيز
            string cookieData = Request.Cookies["Cart"];
            if (!string.IsNullOrEmpty(cookieData))
            {
                var tempItems = System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(cookieData);
                foreach (var item in tempItems)
                {
                    switch (item.Type.ToLower())
                    {
                        case "pc":
                            var pc = _context.PCs.FirstOrDefault(p => p.PCID == item.ProductId);
                            if (pc != null)
                            {
                                item.Name = pc.Brand + " " + pc.Processor;
                                item.ImageURL = pc.ImageURL;
                                item.Price = pc.Price;
                            }
                            break;

                        case "laptop":
                            var laptop = _context.Laptops.FirstOrDefault(l => l.LaptopID == item.ProductId);
                            if (laptop != null)
                            {
                                item.Name = laptop.Brand + " " + laptop.Model;
                                item.ImageURL = laptop.ImageURL;
                                item.Price = laptop.Price;
                            }
                            break;

                        case "part":
                            var part = _context.PCParts.FirstOrDefault(p => p.PartID == item.ProductId);
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
            }

            ViewBag.UserId = 0; // المستخدم مش مسجل
        }

        return View(cartItems);
    }


    [HttpPost]
    public IActionResult AddToCart(CartItemViewModel model)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            // 🟠 المستخدم غير مسجل: خزّن في الكوكيز
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
                HttpOnly = false // لأنك ممكن تحتاج تعدل عليه من js
            };

            Response.Cookies.Append("Cart", System.Text.Json.JsonSerializer.Serialize(cart), options);

            return RedirectToAction("Cart", "Product"); // أفضل يرجع على Cart
        }

        // ✅ المستخدم مسجل: خزّن في قاعدة البيانات
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
        return RedirectToAction("Cart", "Product");
    }



    [HttpPost]
    public IActionResult UpdateQuantity(int ProductId, string Type, int Quantity)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId != null)
        {
            // ✅ مستخدم مسجل → تعديل بالكرت من الداتابيس
            var item = _context.Cart.FirstOrDefault(c => c.UserID == userId && c.ProductId == ProductId && c.Type.ToLower() == Type.ToLower());
            if (item != null)
            {
                item.Quantity = Quantity;
                _context.SaveChanges();
            }
        }
        else
        {
            // 🟠 مستخدم مش مسجل → تعديل بالكرت من الكوكيز
            var cookieData = Request.Cookies["Cart"];
            if (!string.IsNullOrEmpty(cookieData))
            {
                var cart = System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(cookieData)
                           ?? new List<CartItemViewModel>();

                var item = cart.FirstOrDefault(c => c.ProductId == ProductId && c.Type.ToLower() == Type.ToLower());
                if (item != null)
                {
                    item.Quantity = Quantity;

                    var options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(3),
                        IsEssential = true,
                        HttpOnly = false
                    };

                    Response.Cookies.Append("Cart", System.Text.Json.JsonSerializer.Serialize(cart), options);
                }
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
            var item = _context.Cart.FirstOrDefault(c => c.UserID == userId && c.ProductId == ProductId && c.Type.ToLower() == Type.ToLower());
            if (item != null)
            {
                _context.Cart.Remove(item);
                _context.SaveChanges();
            }
        }
        else
        {
            // 🟠 غير المسجلين → حذف من الكوكيز
            var cookieData = Request.Cookies["Cart"];
            if (!string.IsNullOrEmpty(cookieData))
            {
                var cart = System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(cookieData) ?? new();

                var target = cart.FirstOrDefault(c => c.ProductId == ProductId && c.Type.ToLower() == Type.ToLower());
                if (target != null)
                {
                    cart.Remove(target);

                    var options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(3),
                        IsEssential = true,
                        HttpOnly = false
                    };

                    Response.Cookies.Append("Cart", System.Text.Json.JsonSerializer.Serialize(cart), options);
                }
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
            // ✅ المستخدم مسجل → امسح من قاعدة البيانات
            var userCartItems = _context.Cart.Where(c => c.UserID == userId);
            _context.Cart.RemoveRange(userCartItems);
            _context.SaveChanges();
        }
        else
        {
            // 🟠 المستخدم غير مسجل → امسح من الكوكيز
            Response.Cookies.Delete("Cart");
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

    public IActionResult CheckOut()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Authintication");

        // جلب السلة
        var rawCart = _context.Cart
            .Where(c => c.UserID == userId)
            .ToList();

        // 🛑 تحقق إذا السلة فاضية
        if (rawCart == null || !rawCart.Any())
        {
            TempData["EmptyCart"] = "Your cart is empty. Please add items before proceeding to checkout.";
            return RedirectToAction("Cart", "Product"); // أو ارجعه لصفحة السلة Cart
        }

        // جلب المنتجات حسب النوع
        var cartItems = new List<dynamic>();

        foreach (var item in rawCart)
        {
            object product = item.Type switch
            {
                "pc" => _context.PCs.FirstOrDefault(p => p.PCID == item.ProductId),
                "laptop" => _context.Laptops.FirstOrDefault(l => l.LaptopID == item.ProductId),
                _ => _context.PCParts.FirstOrDefault(p => p.PartID == item.ProductId)
            };

            if (product != null)
            {
                cartItems.Add(new
                {
                    item.Type,
                    item.Quantity,
                    Product = product
                });
            }
        }

        ViewBag.Cart = cartItems;
        decimal total = 0;

        foreach (var i in cartItems)
        {
            object product = i.Product;

            if (product is PC pc)
                total += pc.Price * i.Quantity;
            else if (product is Laptop laptop)
                total += laptop.Price * i.Quantity;
            else if (product is PCPart part)
                total += part.Price * i.Quantity;
        }

        ViewBag.Total = total;

        var user = _context.Users.FirstOrDefault(u => u.ID == userId);
        if (user == null)
            return RedirectToAction("Login", "Authintication");

        var model = new CheckoutViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult RemoveFromCartGuest([FromBody] CartItemViewModel model)
    {
        // جلب الكوكيز الحالي
        var cookieData = Request.Cookies["Cart"];
        if (string.IsNullOrEmpty(cookieData))
            return BadRequest("Cart is empty.");

        var cartItems = System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(cookieData);
        var updatedCart = cartItems
            .Where(c => !(c.ProductId == model.ProductId && c.Type.ToLower() == model.Type.ToLower()))
            .ToList();

        // تحديث الكوكيز
        Response.Cookies.Append("Cart", System.Text.Json.JsonSerializer.Serialize(updatedCart), new CookieOptions
        {
            Expires = DateTime.Now.AddDays(3),
            IsEssential = true,
            HttpOnly = false
        });

        return Ok();
    }



    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model, IFormFile OrangeReceipt)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Authintication");

        var cartItems = _context.Cart
            .Where(c => c.UserID == userId)
            .ToList();

        if (!cartItems.Any())
            return BadRequest("Cart is empty");

        decimal total = 0;
        foreach (var item in cartItems)
        {
            decimal price = item.Type switch
            {
                "pc" => _context.PCs.First(p => p.PCID == item.ProductId).Price,
                "laptop" => _context.Laptops.First(l => l.LaptopID == item.ProductId).Price,
                _ => _context.PCParts.First(p => p.PartID == item.ProductId).Price
            };

            total += price * item.Quantity;
        }

        var order = new Order
        {
            UserID = userId.Value,
            TotalPrice = total,
            OrderStatus = "Pending",
            CreatedAt = DateTime.Now
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); 

        string receiptFileName = null;
        if (model.PaymentMethod == "OrangeMoney" && OrangeReceipt != null && OrangeReceipt.Length > 0)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/receipts");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            receiptFileName = Guid.NewGuid() + Path.GetExtension(OrangeReceipt.FileName);
            var filePath = Path.Combine(uploadsPath, receiptFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await OrangeReceipt.CopyToAsync(stream);
            }
        }

        // إنشاء عملية الدفع
        var payment = new Payment
        {
            UserID = userId.Value,
            OrderID = order.OrderID,
            PaymentMethod = model.PaymentMethod,
            Amount = total,
            IsPaid = true,
            PaymentDate = DateTime.Now,
            ReceiptImage = receiptFileName
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // تفريغ السلة
        _context.Cart.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return RedirectToAction("MyOrders", "Authintication");
    }




}




