using Master_pice.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Master_pice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserAdminController : Controller
    {
        private readonly AppDbContext _context;

        public UserAdminController(AppDbContext context)
        {
            _context = context;
        }

        // Action to show all users
        public IActionResult ViewAllUsers()
        {
            var users = _context.Users.ToList(); // Fetch all users from the Users table
            return View(users); // Pass the users list to the View
        }

        public IActionResult ViewUserDetails(int id)
        {
            // جلب المستخدم بناءً على الـ ID
            var user = _context.Users.FirstOrDefault(u => u.ID == id);

            if (user == null)
            {
                return NotFound();  // إذا كان المستخدم غير موجود
            }

            return View(user);  // تمرير البيانات إلى الفيو
        }

    }
}
