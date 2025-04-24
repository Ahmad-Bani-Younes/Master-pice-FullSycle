using Master_pice.Models;
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
            model.RegionOptions = GetJordanRegions(); // ✅ إعادة تعبئة المناطق في POST

            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
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
                    Region = model.Region // ✅ إضافة المنطقة
                };

                _context.Users.Add(user);
                _context.SaveChanges();

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


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Login(LoginViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
        //        if (user != null)
        //        {
        //            HttpContext.Session.SetInt32("UserId", user.ID);
        //            HttpContext.Session.SetString("UserName", user.FullName);
        //            HttpContext.Session.SetString("UserType", user.UserType);
        //            HttpContext.Session.SetString("UserImage", user.ProfileImage ?? "");

        //            if (model.RememberMe)
        //            {
        //                CookieOptions options = new CookieOptions
        //                {
        //                    Expires = DateTimeOffset.UtcNow.AddDays(7)
        //                };

        //                Response.Cookies.Append("RememberMe_UserId", user.ID.ToString(), options);
        //            }

        //            return RedirectToAction("Index", "Home");
        //        }

        //        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        //    }

        //    return View(model);
        //}




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

                    if (model.RememberMe)
                    {
                        CookieOptions options = new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddDays(7)
                        };

                        Response.Cookies.Append("RememberMe_UserId", user.ID.ToString(), options);
                    }

                    // 🔁 دمج سلة الكوكيز مع قاعدة البيانات
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

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(model);
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
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

            return RedirectToAction("Profile");
        }


        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendResetLink(string email)
        {
            //var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            //if (user == null)
            //    return NotFound("Email not found");

            //var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            //// هنا ممكن تخزنه بجدول أو ببساطة تبعت Id مشفر
            //var resetLink = Url.Action("ResetPassword", "Auth", new { userId = user.ID }, Request.Scheme);

            //// 📨 إرسال الإيميل (استخدم SMTP أو MailKit)
            //await EmailSender.SendAsync(email, "Reset your password", $"Click <a href='{resetLink}'>here</a> to reset your password");

            //return View("CheckEmail"); // صفحة بسيطة تقول راجع بريدك


            // جلب اليوزر
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("Email not found");

            // مؤقتًا: إعادة توجيه مباشر لصفحة ResetPassword
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



        public async Task<IActionResult> MyOrders()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Authintication");
            }


            var orders = await _context.Orders
                .Where(o => o.UserID == userId)
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
                      })
                .ToListAsync();

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
                TempData["Message"] = "Order cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "You can only cancel within 24 hours.";
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

            // تحقق من كلمة المرور القديمة
            if (user.Password != model.OldPassword)
            {
                ModelState.AddModelError("OldPassword", "Incorrect old password.");
                return View(model);
            }

            // تحديث كلمة المرور
            user.Password = model.NewPassword;
            _context.SaveChanges();

            TempData["Success"] = "Password updated successfully!";
            return RedirectToAction("Profile");
        }






    }


}

