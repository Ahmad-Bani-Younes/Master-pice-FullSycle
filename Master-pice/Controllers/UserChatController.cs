using Master_pice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master_pice.Controllers
{
    public class UserChatController : Controller
    {
        private readonly AppDbContext _context;
        public UserChatController(AppDbContext context) => _context = context;

        public IActionResult ChatWithAdmin()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.ID == userId);
            ViewBag.UserName = user?.FullName ?? "User";
            if (userId == null) return RedirectToAction("Login", "Authintication");

            var messages = _context.ChatMessages
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.SentAt)
                .ToList();

            //var user = _context.Users.FirstOrDefault(u => u.ID == userId);
            ViewBag.FullName = user?.FullName;
            ViewBag.UserId = userId;

            return View(messages);
        }



        [HttpPost]
        public IActionResult SendMessageUser(int userId, string message, IFormFile image)
        {
            if (string.IsNullOrWhiteSpace(message) && (image == null || image.Length == 0))
            {
                TempData["Error"] = "Please enter a message or upload an image.";
                return RedirectToAction("ChatWithAdmin");
            }

            string imageName = null;

            if (image != null && image.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                imageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                string filePath = Path.Combine(uploadsFolder, imageName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                message = "[Image]";
            }

            var chat = new ChatMessage
            {
                UserId = userId,
                Message = message,
                SenderType = "User",
                ImagePath = imageName,
                SentAt = DateTime.Now
            };

            _context.ChatMessages.Add(chat);
            _context.SaveChanges();

            return RedirectToAction("ChatWithAdmin");
        }



    }
}
