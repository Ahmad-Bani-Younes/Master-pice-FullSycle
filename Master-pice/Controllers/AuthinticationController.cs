using Master_pice.Models;
using Master_pice.ViewModel; 
using Microsoft.AspNetCore.Mvc;

namespace Master_pice.Controllers
{
    public class AuthinticationController : Controller
    {
        private readonly AppDbContext _context;

        public AuthinticationController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
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
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(model);
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



                    return RedirectToAction("Index","Home");
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





    }
}
