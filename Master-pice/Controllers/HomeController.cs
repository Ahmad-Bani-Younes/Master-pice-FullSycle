using Master_pice.Models;
using Master_pice.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Master_pice.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 20;

            ViewBag.HotAds = _context.PCs
                .OrderByDescending(p => p.Price)
                .Take(3)
                .ToList();

            ViewBag.Desktops = _context.PCs
                .OrderByDescending(p => p.Price)
                .Take(8)
                .ToList();

            ViewBag.Laptops = _context.Laptops
                .OrderByDescending(p => p.Price)
                .Take(8)
                .ToList();

            ViewBag.Parts = _context.PCParts
                .OrderByDescending(p => p.Price)
                .Take(8)
                .ToList();

       
            var allProducts = _context.PCs
                .Select(p => new ProductViewModel
                {
                    ID = p.PCID,
                    Name = p.Brand + " " + p.Processor,
                    Description = p.Description,
                    Price = p.Price,
                    ImageURL = p.ImageURL,
                    Type = "pc"
                })
                .Concat(_context.Laptops.Select(l => new ProductViewModel
                {
                    ID = l.LaptopID,
                    Name = l.Brand + " " + l.Model,
                    Description = l.Description,
                    Price = l.Price,
                    ImageURL = l.ImageURL,
                    Type = "laptop"
                }))
                .Concat(_context.PCParts.Select(part => new ProductViewModel
                {
                    ID = part.PartID,
                    Name = part.Model,
                    Description = "", 
                    Price = part.Price,
                    ImageURL = part.ImageURL,
                    Type = "pcpart"
                }))
                .ToList();

            int totalProducts = allProducts.Count();
            var products = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentPage = page;

            return View(products);
        }


        public IActionResult About()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
