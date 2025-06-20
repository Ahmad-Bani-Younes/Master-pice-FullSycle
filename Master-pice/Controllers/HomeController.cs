﻿using Master_pice.Models;
using Master_pice.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Master_pice.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                var rememberedUserId = Request.Cookies["RememberMe_UserId"];
                if (!string.IsNullOrEmpty(rememberedUserId))
                {
                    var user = _context.Users.FirstOrDefault(u => u.ID == int.Parse(rememberedUserId));
                    if (user != null)
                    {
                        HttpContext.Session.SetInt32("UserId", user.ID);
                        HttpContext.Session.SetString("UserName", user.FullName);
                        HttpContext.Session.SetString("UserType", user.UserType);
                        HttpContext.Session.SetString("UserImage", user.ProfileImage ?? "");
                    }
                }
            }

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
            var sections = _context.AboutContents.OrderBy(x => x.CreatedAt).ToList();

            var aboutReviews = _context.Ratings
                .OrderByDescending(r => r.CreatedAt)
                .Take(10) 
                .Select(r => new
                {
                    UserName = r.User.FullName,
                    UserImage = r.User.ProfileImage ?? "default-user.png",
                    r.Comment,
                    r.Stars,
                    ProductName = r.ProductType == "pc"
                        ? _context.PCs.Where(p => p.PCID == r.ProductId).Select(p => p.Brand + " " + p.Processor).FirstOrDefault()
                        : r.ProductType == "laptop"
                            ? _context.Laptops.Where(l => l.LaptopID == r.ProductId).Select(l => l.Brand + " " + l.Model).FirstOrDefault()
                            : _context.PCParts.Where(p => p.PartID == r.ProductId).Select(p => p.Model).FirstOrDefault(),
                    ProductImage = r.ProductType == "pc"
                        ? _context.PCs.Where(p => p.PCID == r.ProductId).Select(p => p.ImageURL).FirstOrDefault()
                        : r.ProductType == "laptop"
                            ? _context.Laptops.Where(l => l.LaptopID == r.ProductId).Select(l => l.ImageURL).FirstOrDefault()
                            : _context.PCParts.Where(p => p.PartID == r.ProductId).Select(p => p.ImageURL).FirstOrDefault()
                })
                .ToList();

            ViewBag.AboutReviews = aboutReviews;
            return View(sections);
        }


        public IActionResult ContactUs()
        {
            var viewModel = new ContactMessageViewModel();

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.ID == userId);

                if (user != null)
                {
                    var fullNameParts = user.FullName.Split(' ', 2);
                    viewModel.FirstName = fullNameParts[0];
                    viewModel.LastName = fullNameParts.Length > 1 ? fullNameParts[1] : "";
                    viewModel.Email = user.Email;
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SendMessage(ContactMessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var message = new ContactMessage
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Message = model.Message
                };

                _context.ContactMessages.Add(message);
                _context.SaveChanges();

                SuccessMessage = "Your message has been sent successfully!";
                return RedirectToAction("ContactUs");
            }

            return View("ContactUs", model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ErrorMessage = "An unexpected error occurred.";
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            var laptops = await _context.Laptops
                .Where(l => l.Brand.Contains(query) || l.Model.Contains(query) || l.Description.Contains(query))
                .Select(l => new SearchResultViewModel
                {
                    Id = l.LaptopID,  
                    Name = l.Brand + " " + l.Model,
                    Description = l.Description,
                    Price = l.Price,
                    ImageURL = l.ImageURL,
                    Type = "Laptop"
                })
                .ToListAsync();

            var pcs = await _context.PCs
                .Where(p => p.Brand.Contains(query) || p.Description.Contains(query))
                .Select(p => new SearchResultViewModel
                {
                    Id = p.PCID,   
                    Name = p.Brand,
                    Description = p.Description,
                    Price = p.Price,
                    ImageURL = p.ImageURL,
                    Type = "PC"
                })
                .ToListAsync();

            var pcParts = await _context.PCParts
                .Where(pp => pp.Category.Contains(query) || pp.Model.Contains(query))
                .Select(pp => new SearchResultViewModel
                {
                    Id = pp.PartID,   
                    Name = pp.Category,
                    Description = pp.Model,
                    Price = pp.Price,
                    ImageURL = pp.ImageURL,
                    Type = "PC Part"
                })
                .ToListAsync();

            var allResults = laptops.Concat(pcs).Concat(pcParts).ToList();

            ViewBag.SearchQuery = query;
            return View(allResults);
        }




    }
}
