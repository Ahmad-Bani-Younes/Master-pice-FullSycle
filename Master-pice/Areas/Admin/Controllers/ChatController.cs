using Master_pice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master_pice.Controllers
{
    [Area("Admin")]
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;
        public ChatController(AppDbContext context) => _context = context;

        public IActionResult ChatAdmin()
        {
            var grouped = _context.ChatMessages
                .Include(c => c.User)
                .GroupBy(m => m.UserId)
                .Select(g => new {
                    UserId = g.Key,
                    UserName = g.First().User.FullName,
                    LastMessage = g.OrderByDescending(x => x.SentAt).First().Message,
                    LastTime = g.OrderByDescending(x => x.SentAt).First().SentAt
                }).ToList();

            return View(grouped);
        }

        public IActionResult ChatWithUser(int userId)
        {
            var messages = _context.ChatMessages
                .Where(m => m.UserId == userId)
                .Include(m => m.User)
                .OrderBy(m => m.SentAt)
                .ToList();

            var user = _context.Users.FirstOrDefault(u => u.ID == userId);
            ViewBag.FullName = user?.FullName ?? "User";
            ViewBag.UserId = userId;

            return View(messages);
        }


        [HttpPost]
        public IActionResult SendMessageUser(int userId, string message, IFormFile image)
        {
            // ✅ إذا ما في رسالة ولا صورة، ما نكمل
            if (string.IsNullOrWhiteSpace(message) && (image == null || image.Length == 0))
            {
                TempData["Error"] = "Please enter a message or upload an image.";
                return RedirectToAction("ChatWithUser", new { userId });
            }

            string imageName = null;

            // ✅ حفظ الصورة إذا وُجدت
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

            // ✅ تعيين رسالة افتراضية إذا كانت فاضية
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "[Image]";
            }

            var chat = new ChatMessage
            {
                UserId = userId,
                Message = message,
                SenderType = "Admin",
                ImagePath = imageName,
                SentAt = DateTime.Now
            };

            _context.ChatMessages.Add(chat);
            _context.SaveChanges();

            return RedirectToAction("ChatWithUser", new { userId });
        }

    }
}
