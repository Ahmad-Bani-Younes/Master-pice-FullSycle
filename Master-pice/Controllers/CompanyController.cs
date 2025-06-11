using Master_pice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master_pice.Controllers
{
    public class CompanyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CompanyController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ====================== [ Register ] ======================
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Company company, IFormFile Logo, IFormFile BusinessLicense)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Companies.AnyAsync(c => c.Email == company.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(company);
                }

                // Save logo file
                if (Logo != null && Logo.Length > 0)
                {
                    var logoFileName = Guid.NewGuid() + Path.GetExtension(Logo.FileName);
                    var logoPath = Path.Combine(_env.WebRootPath, "uploads/logos", logoFileName);
                    using (var stream = new FileStream(logoPath, FileMode.Create))
                    {
                        await Logo.CopyToAsync(stream);
                    }
                    company.LogoPath = "/uploads/logos/" + logoFileName;
                }

                // Save business license
                if (BusinessLicense != null && BusinessLicense.Length > 0)
                {
                    var licenseFileName = Guid.NewGuid() + Path.GetExtension(BusinessLicense.FileName);
                    var licensePath = Path.Combine(_env.WebRootPath, "uploads/licenses", licenseFileName);
                    using (var stream = new FileStream(licensePath, FileMode.Create))
                    {
                        await BusinessLicense.CopyToAsync(stream);
                    }
                    company.BusinessLicensePath = "/uploads/licenses/" + licenseFileName;
                }

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Company registered successfully!";
                return RedirectToAction("Login");
            }

            return View(company);
        }

        // ====================== [ Login ] ======================
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            var company = await _context.Companies
                .Where(c => c.Email != null && c.PasswordHash != null)
                .Where(c => c.Email == email && c.PasswordHash == password)
                .Select(c => new { c.CompanyId, c.CompanyName, c.LogoPath }) 
                .FirstOrDefaultAsync();

            if (company == null)
            {
                ViewBag.Error = "Invalid login credentials.";
                return View();
            }

            HttpContext.Session.SetInt32("CompanyId", company.CompanyId);
            HttpContext.Session.SetString("CompanyName", company.CompanyName ?? "Company");
            HttpContext.Session.SetString("CompanyLogo", company.LogoPath ?? "/images/company-default.png"); // ✅ أضف هذا

            HttpContext.Session.SetString("UserRole", "Company");

            return RedirectToAction("Index", "Home");
        }



        // ====================== [ Dashboard ] ======================
        public async Task<IActionResult> Dashboard()
        {
            int? companyId = HttpContext.Session.GetInt32("CompanyId");

            if (companyId == null)
                return RedirectToAction("Login");

            var company = await _context.Companies
                .Include(c => c.Laptops)
                .Include(c => c.PCs)
                .Include(c => c.PCParts)
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            return View(company);
        }

        // ====================== [ Logout ] ======================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
