﻿using Master_pice.Models;
using Master_pice.ViewModel;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;

public class ProductController : Controller
{
    private readonly AppDbContext _context;
    private readonly IAntiforgery _antiforgery;

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

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

        // 1. جلب الريفيوز من الداتا بيس
        var reviews = _context.Ratings
            .Where(r => r.ProductId == id && r.ProductType == type)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Stars,
                r.Comment,
                r.CreatedAt,
                UserName = r.User.FullName,
                UserImage = r.User.ProfileImage
            })
            .ToList();

        // 2. فحص إذا في تقييم مؤقت مخزن بالكوكيز
        if (Request.Cookies.TryGetValue("TempReview", out var tempReviewJson))
        {
            var tempReview = System.Text.Json.JsonSerializer.Deserialize<TempReviewModel>(tempReviewJson);

            if (tempReview != null && tempReview.ProductId == id && tempReview.Type == type)
            {
                // ✨ أضف الريفيو تبع الكوكيز إلى قائمة الريفيوز الأصلية
                reviews.Insert(0, new
                {
                    Stars = tempReview.Stars,
                    Comment = tempReview.Comment,
                    CreatedAt = tempReview.CreatedAt,
                    UserName = "Guest User",
                    UserImage = "default-user.png"
                });
            }
        }

        // 3. ربطهم بالـ ViewBag
        ViewBag.Reviews = reviews;


        return View(product);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitReview(int ProductId, string Type, int Stars, string Comment)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (Stars < 1 || Stars > 5)
        {
            TempData["ErrorMessage"] = "Please select a valid rating.";
            return RedirectToAction("Details", new { id = ProductId, type = Type });
        }

        if (userId == null)
        {
            // 🧠 تخزين التقييم في الكوكيز
            var tempReview = new TempReviewModel
            {
                ProductId = ProductId,
                Type = Type,
                Stars = Stars,
                Comment = Comment,
                CreatedAt = DateTime.Now
            };

            string tempReviewJson = System.Text.Json.JsonSerializer.Serialize(tempReview);

            CookieOptions options = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(30), // تخليه نص ساعة مثلاً
                IsEssential = true
            };

            Response.Cookies.Append("TempReview", tempReviewJson, options);

            TempData["SuccessMessage"] = "Review saved temporarily! Please login to submit it.";
            return RedirectToAction("Details", new { id = ProductId, type = Type });
        }

        var review = new Rating
        {
            UserId = userId.Value,
            ProductId = ProductId,
            ProductType = Type,
            Stars = Stars,
            Comment = Comment,
            CreatedAt = DateTime.Now
        };

        _context.Ratings.Add(review);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Thank you for your review!";
        return RedirectToAction("Details", new { id = ProductId, type = Type });
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
        decimal shipping = 3.0m;

        if (userId != null)
        {
            var user = _context.Users.FirstOrDefault(u => u.ID == userId);
            if (user?.Region?.Trim().ToLower() == "irbid")
            {
                shipping = 2.0m;
            }
        }

        ViewBag.Shipping = shipping;
        ViewBag.Subtotal = cartItems.Sum(i => i.Price * i.Quantity);
        ViewBag.Total = ViewBag.Subtotal + shipping;


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

    public IActionResult CheckOut(int? productId, string type)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            if (productId != null && !string.IsNullOrEmpty(type))
            {
                HttpContext.Session.SetInt32("TempProductId", productId.Value);
                HttpContext.Session.SetString("TempProductType", type);
            }
            return RedirectToAction("Login", "Authintication");
        }

        if (productId == null && string.IsNullOrEmpty(type))
        {
            productId = HttpContext.Session.GetInt32("TempProductId");
            type = HttpContext.Session.GetString("TempProductType");

            HttpContext.Session.Remove("TempProductId");
            HttpContext.Session.Remove("TempProductType");
        }

        List<dynamic> cartItems = new List<dynamic>();

        if (productId != null && !string.IsNullOrEmpty(type))
        {
            object product = type switch
            {
                "pc" => _context.PCs.FirstOrDefault(p => p.PCID == productId),
                "laptop" => _context.Laptops.FirstOrDefault(l => l.LaptopID == productId),
                _ => _context.PCParts.FirstOrDefault(p => p.PartID == productId)
            };

            if (product != null)
            {
                cartItems.Add(new
                {
                    Type = type,
                    Quantity = 1,
                    Product = product
                });

                // ✅ هنا حفظنا المنتج مؤقتًا داخل الجلسة عشان لما نروح على PlaceOrder نلاقيه
                var tempCart = new List<CartItemViewModel>
            {
                new CartItemViewModel
                {
                    ProductId = productId.Value,
                    Type = type,
                    Quantity = 1
                }
            };
                var tempCartJson = System.Text.Json.JsonSerializer.Serialize(tempCart);

                HttpContext.Session.SetString("TempCart", tempCartJson);
            }
        }
        else
        {
            // ✅ جاي من الكارت العادي
            var rawCart = _context.Cart
                .Where(c => c.UserID == userId)
                .ToList();

            if (rawCart == null || !rawCart.Any())
            {
                TempData["EmptyCart"] = "Your cart is empty. Please add items before proceeding to checkout.";
                return RedirectToAction("Cart", "Product");
            }

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

            // ✅ احذف أي بيانات شراء مباشر قديمة إذا فتح الشيك آوت عن طريق الكارت
            HttpContext.Session.Remove("TempCart");
        }

        ViewBag.Cart = cartItems;

        decimal subtotal = 0;
        foreach (var i in cartItems)
        {
            if (i.Product is PC pc)
                subtotal += pc.Price * i.Quantity;
            else if (i.Product is Laptop laptop)
                subtotal += laptop.Price * i.Quantity;
            else if (i.Product is PCPart part)
                subtotal += part.Price * i.Quantity;
        }

        var user = _context.Users.FirstOrDefault(u => u.ID == userId);
        if (user == null)
            return RedirectToAction("Login", "Authintication");

        decimal shipping = user?.Region?.Trim().ToLower() == "irbid" ? 2.0m : 3.0m;
        decimal total = subtotal + shipping;

        ViewBag.Subtotal = subtotal;
        ViewBag.Shipping = shipping;
        ViewBag.Total = total;

        var model = new CheckoutViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Region = user.Region
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
        if (userId == null)
            return RedirectToAction("Login", "Authintication");

        List<CartItemViewModel> cartItems = new List<CartItemViewModel>();

        // ✅ تحقق أولاً: هل يوجد شراء مباشر مخزن بجلسة TempCart؟
        var tempCartJson = HttpContext.Session.GetString("TempCart");
        if (!string.IsNullOrEmpty(tempCartJson))
        {
            cartItems = System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(tempCartJson);
        }
        else
        {
            // ✅ لا يوجد شراء مباشر - اعتمد على السلة Cart
            cartItems = _context.Cart
                .Where(c => c.UserID == userId)
                .Select(c => new CartItemViewModel
                {
                    ProductId = c.ProductId,
                    Type = c.Type,
                    Quantity = c.Quantity
                })
                .ToList();
        }

        if (cartItems == null || !cartItems.Any())
            return Content("Cart is empty");

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

        // ✅ الآن تفريغ الكارت فقط إذا لم يكن شراء مباشر
        if (string.IsNullOrEmpty(tempCartJson))
        {
            var userCart = _context.Cart.Where(c => c.UserID == userId).ToList();
            if (userCart.Any())
            {
                _context.Cart.RemoveRange(userCart);
                await _context.SaveChangesAsync();
            }
        }
        else
        {
            // إذا الشراء مباشر خلينا نمسح الجلسة فقط بدون تعديل الكارت
            HttpContext.Session.Remove("TempCart");
        }

        return RedirectToAction("MyOrders", "Authintication");
    }




}




