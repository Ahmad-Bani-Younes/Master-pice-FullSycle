﻿using Master_pice.Models;
using Master_pice.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Master_pice.Controllers
{
    public class AuthinticationController : Controller
    {
        private readonly AppDbContext _context;

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public AuthinticationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterViewModel
            {
                RegionOptions = GetJordanRegions()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            model.RegionOptions = GetJordanRegions();

            if (ModelState.IsValid)
            {
                // ✅ تحقق إذا الإيميل موجود مسبقا
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                // ✅ تحقق إذا رقم التلفون موجود مسبقا
                if (_context.Users.Any(u => u.Phone == model.Phone))
                {
                    ModelState.AddModelError("Phone", "This phone number is already registered.");
                    return View(model);
                }

                string imageName = null;
                if (model.ProfileImage != null)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, imageName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ProfileImage.CopyTo(stream);
                    }
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,
                    Phone = model.Phone,
                    UserType = model.UserType,
                    ProfileImage = imageName,
                    CreatedAt = DateTime.Now,
                    Region = model.Region
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                SuccessMessage = "Account created successfully. Please login.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        private List<SelectListItem> GetJordanRegions()
        {
            return new List<SelectListItem>
            {
                new("Amman", "Amman"),
                new("Zarqa", "Zarqa"),
                new("Irbid", "Irbid"),
                new("Balqa", "Balqa"),
                new("Aqaba", "Aqaba"),
                new("Ma'an", "Ma'an"),
                new("Karak", "Karak"),
                new("Tafilah", "Tafilah"),
                new("Madaba", "Madaba"),
                new("Ajloun", "Ajloun"),
                new("Jerash", "Jerash"),
                new("Mafraq", "Mafraq")
            };
        }

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.ID);
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("UserType", user.UserType);
                    HttpContext.Session.SetString("UserImage", user.ProfileImage ?? "");


                    // تحقق إذا كان هناك تقييم معلق بالكوكيز
                    if (Request.Cookies.TryGetValue("PendingReview", out var pendingReviewJson))
                    {
                        var tempReview = System.Text.Json.JsonSerializer.Deserialize<TempReviewModel>(pendingReviewJson);

                        if (tempReview != null)
                        {
                            var newRating = new Rating
                            {
                                UserId = user.ID,
                                ProductId = tempReview.ProductId,
                                ProductType = tempReview.Type,
                                Stars = tempReview.Stars,
                                Comment = tempReview.Comment,
                                CreatedAt = tempReview.CreatedAt
                            };

                            _context.Ratings.Add(newRating);
                            _context.SaveChanges();
                            Response.Cookies.Delete("PendingReview");
                        }
                    }




                    if (model.RememberMe)
                    {
                        CookieOptions options = new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddDays(7)
                        };

                        Response.Cookies.Append("RememberMe_UserId", user.ID.ToString(), options);
                        Response.Cookies.Append("RememberMe_Email", user.Email, options);
                        Response.Cookies.Append("RememberMe_Password", user.Password, options);
                    }

                    if (Request.Cookies.TryGetValue("Cart", out string cartJson) && !string.IsNullOrEmpty(cartJson))
                    {
                        var cookieCart = JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson);
                        foreach (var item in cookieCart)
                        {
                            var existing = _context.Cart.FirstOrDefault(c =>
                                c.UserID == user.ID &&
                                c.ProductId == item.ProductId &&
                                c.Type == item.Type);

                            if (existing != null)
                                existing.Quantity += item.Quantity;
                            else
                                _context.Cart.Add(new Cart
                                {
                                    UserID = user.ID,
                                    ProductId = item.ProductId,
                                    Type = item.Type,
                                    Quantity = item.Quantity
                                });
                        }

                        _context.SaveChanges();
                        Response.Cookies.Delete("Cart");
                    }

                    SuccessMessage = "Login successful!";

                    int? tempProductId = HttpContext.Session.GetInt32("TempProductId");
                    string tempProductType = HttpContext.Session.GetString("TempProductType");

                    if (tempProductId != null && !string.IsNullOrEmpty(tempProductType))
                    {
                        HttpContext.Session.Remove("TempProductId");
                        HttpContext.Session.Remove("TempProductType");

                        return RedirectToAction("CheckOut", "Product", new { productId = tempProductId, type = tempProductType });
                    }

                    if (user.Email == "ahmadbaniyounes542@gmail.com" && user.Password == "Ahmad123")
                    {
                        return RedirectToAction("Index", "Admin"); 
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home"); 
                    }
                }

                TempData["LoginError"] = "Invalid email or password.";
            }

            return View(model);
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            SuccessMessage = "Logged out successfully.";
            return RedirectToAction("Login", "Authintication");
        }

        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.ID == userId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.ID == userId);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(User model, IFormFile NewProfileImage)
        {
            var user = _context.Users.FirstOrDefault(u => u.ID == model.ID);
            if (user == null) return RedirectToAction("Login");

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Region = model.Region;

            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = model.Password;
            }

            if (NewProfileImage != null)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                var imageName = Guid.NewGuid().ToString() + Path.GetExtension(NewProfileImage.FileName);
                var filePath = Path.Combine(uploads, imageName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    NewProfileImage.CopyTo(stream);
                }

                user.ProfileImage = imageName;
                HttpContext.Session.SetString("UserImage", imageName);
            }

            _context.SaveChanges();
            HttpContext.Session.SetString("UserName", user.FullName);

            SuccessMessage = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendResetLink(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ErrorMessage = "Email not found.";
                return RedirectToAction("ForgetPassword");
            }

            return RedirectToAction("ResetPassword", "Authintication", new { userId = user.ID });
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(int userId)
        {
            var model = new ResetPasswordViewModel
            {
                UserId = userId
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null) return NotFound();

            user.Password = model.Password;
            await _context.SaveChangesAsync();

            SuccessMessage = "Password reset successfully. Please login.";
            return RedirectToAction("Login");
        }

        public static class EmailSender
        {
            public static async Task SendAsync(string to, string subject, string body)
            {
                var client = new SmtpClient("smtp.yourserver.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("your@email.com", "your-password"),
                    EnableSsl = true,
                };

                await client.SendMailAsync(new MailMessage("your@email.com", to, subject, body)
                {
                    IsBodyHtml = true
                });
            }
        }

        public async Task<IActionResult> MyOrders(int? take)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Authintication");
            }

            var ordersQuery = _context.Orders
                .Where(o => o.UserID == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Join(_context.Payments,
                      order => order.OrderID,
                      payment => payment.OrderID,
                      (order, payment) => new OrderWithPaymentViewModel
                      {
                          OrderID = order.OrderID,
                          TotalPrice = order.TotalPrice,
                          OrderStatus = order.OrderStatus,
                          CreatedAt = order.CreatedAt,
                          PaymentMethod = payment.PaymentMethod
                      });

            if (take.HasValue)
            {
                ordersQuery = ordersQuery.Take(take.Value);
            }

            var orders = await ordersQuery.ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public IActionResult CancelOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (order == null) return NotFound();

            if ((DateTime.Now - order.CreatedAt).TotalHours < 24)
            {
                order.OrderStatus = "Cancelled";
                _context.SaveChanges();
                SuccessMessage = "Order cancelled successfully.";
            }
            else
            {
                ErrorMessage = "You can only cancel within 24 hours.";
            }

            return RedirectToAction("MyOrders");
        }

        [HttpGet]
        public IActionResult ResetPasswordAfterLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPasswordAfterLogin(ResetPasswordViewModelAfterLogin model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.ID == userId);
            if (user == null)
                return NotFound();

            if (user.Password != model.OldPassword)
            {
                ErrorMessage = "Incorrect old password.";
                return View(model);
            }

            user.Password = model.NewPassword;
            _context.SaveChanges();

            SuccessMessage = "Password updated successfully!";
            return RedirectToAction("Profile");
        }



        public IActionResult MyReviews()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Authintication");

            var userRatings = _context.Ratings
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            var reviewDetails = new List<dynamic>();

            foreach (var rating in userRatings)
            {
                string productName = "";
                string productImage = "";

                switch (rating.ProductType.ToLower())
                {
                    case "pc":
                        var pc = _context.PCs.FirstOrDefault(p => p.PCID == rating.ProductId);
                        if (pc != null)
                        {
                            productName = pc.Brand + " " + pc.Processor;
                            productImage = pc.ImageURL;
                        }
                        break;
                    case "laptop":
                        var laptop = _context.Laptops.FirstOrDefault(l => l.LaptopID == rating.ProductId);
                        if (laptop != null)
                        {
                            productName = laptop.Brand + " " + laptop.Model;
                            productImage = laptop.ImageURL;
                        }
                        break;
                    case "pcpart":
                        var part = _context.PCParts.FirstOrDefault(p => p.PartID == rating.ProductId);
                        if (part != null)
                        {
                            productName = part.Model;
                            productImage = part.ImageURL;
                        }
                        break;
                }

                reviewDetails.Add(new
                {
                    ProductId = rating.ProductId,          // ✅ مهم جدا إضافته
                    ProductType = rating.ProductType,       // ✅ ومهم جدا أيضا إضافته
                    ProductName = productName,
                    ProductImage = productImage ?? "no-image.png",
                    Stars = rating.Stars,
                    Comment = rating.Comment,
                    CreatedAt = rating.CreatedAt
                });
            }

            return View(reviewDetails);
        }





    }
}
