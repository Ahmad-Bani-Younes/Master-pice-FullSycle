using Master_pice.Models;
using Microsoft.AspNetCore.Mvc;

namespace Master_pice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AboutUsAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AboutUsAdminController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult ViewAboutContent()
        {
            var sections = _context.AboutContents.OrderByDescending(x => x.CreatedAt).ToList();
            return View(sections);
        }

        [HttpGet]
        public IActionResult CreateAboutContent()
        {
            return View(new AboutContent()); 
        }

        [HttpPost]
        public IActionResult CreateAboutContent(AboutContent model, IFormFile Image)
        {
            if (Image != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                Image.CopyTo(stream);
                model.ImageUrl = fileName;
            }

            _context.AboutContents.Add(model);
            _context.SaveChanges();
            return RedirectToAction("ViewAboutContent");
        }

        [HttpGet]
        public IActionResult EditAboutContent(int id)
        {
            var section = _context.AboutContents.Find(id);
            return View(section);
        }

        [HttpPost]
        public IActionResult EditAboutContent(AboutContent model, IFormFile Image)
        {
            var section = _context.AboutContents.Find(model.Id);

            if (Image != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                Image.CopyTo(stream);
                section.ImageUrl = fileName;
            }

            section.Title = model.Title;
            section.Description = model.Description;
            _context.SaveChanges();
            return RedirectToAction("ViewAboutContent");
        }

        public IActionResult Delete(int id)
        {
            var section = _context.AboutContents.Find(id);
            if (section != null)
            {
                _context.AboutContents.Remove(section);
                _context.SaveChanges();
            }
            return RedirectToAction("ViewAboutContent");
        }
    }

}
