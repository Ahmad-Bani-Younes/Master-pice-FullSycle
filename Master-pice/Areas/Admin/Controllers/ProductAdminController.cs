using Master_pice.Models;
using Master_pice.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Master_pice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductAdminController : Controller
    {
        private readonly AppDbContext _context;

        public ProductAdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var laptops = _context.Laptops.Select(p => new ProductViewModel
            {
                ID = p.LaptopID,
                Name = p.Model,
                Description = p.Description,
                Price = p.Price,
                ImageURL = p.ImageURL,
                Type = "Laptop"
            });

            var pcs = _context.PCs.Select(p => new ProductViewModel
            {
                ID = p.PCID,
                Name = p.Brand + " PC",
                Description = p.Description,
                Price = p.Price,
                ImageURL = p.ImageURL,
                Type = "PC"
            });

            var parts = _context.PCParts.Select(p => new ProductViewModel
            {
                ID = p.PartID,
                Name = p.Model,
                Description = p.Compatibility,
                Price = p.Price,
                ImageURL = p.ImageURL,
                Type = "PCPart"
            });

            var all = laptops.Concat(pcs).Concat(parts).ToList();
            return View(all);
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }


        [HttpPost]
        public IActionResult AddProduct([FromForm] ProductViewModel model, IFormFile ImageFile, List<IFormFile> AdditionalImages)
        {
            if (string.IsNullOrWhiteSpace(model.Name) ||
                string.IsNullOrWhiteSpace(model.Type) ||
                string.IsNullOrWhiteSpace(model.Description) ||
                model.Price <= 0)
            {
                ViewBag.Error = "All fields are required and must be valid.";
                return View(model);
            }

            // تحديد مجلد التحميل
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);  // التأكد من أن المجلد موجود

            // حفظ الصورة الرئيسية
            string imageName = null;
            if (ImageFile != null && ImageFile.Length > 0)
            {
                imageName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, imageName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                     ImageFile.CopyToAsync(stream);
                }
            }

            List<string> additionalImageUrls = new List<string>();
            if (AdditionalImages != null && AdditionalImages.Count > 0)
            {
                foreach (var image in AdditionalImages)
                {
                    if (image.Length > 0)
                    {
                        string additionalImageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        var additionalFilePath = Path.Combine(uploadsFolder, additionalImageName);
                        using (var stream = new FileStream(additionalFilePath, FileMode.Create))
                        {
                             image.CopyToAsync(stream);
                        }
                        additionalImageUrls.Add(additionalImageName); 

                    }
                }
            }

            // إضافة المنتج بناءً على النوع
            switch (model.Type.ToLower())
            {
                case "laptop":
                    _context.Laptops.Add(new Laptop
                    {
                        Model = model.Name,
                        Description = model.Description,
                        Price = model.Price,
                        ImageURL = imageName, 
                        AdditionalImageURLs = string.Join(",", additionalImageUrls), 
                        Brand = model.Brand,
                        Processor = model.Processor,
                        RAM = model.RAM,
                        Storage = model.Storage,
                        GPU = model.GPU,
                        ScreenSize = model.ScreenSize,
                        Stock = model.Stock
                    });
                    break;

                case "pc":
                    _context.PCs.Add(new PC
                    {
                        Brand = model.Name,
                        Description = model.Description,
                        Price = model.Price,
                        ImageURL = imageName,
                        AdditionalImageURLs = string.Join(",", additionalImageUrls),
                        Processor = model.Processor,
                        RAM = model.RAM,
                        Storage = model.Storage,
                        GPU = model.GPU,
                        Stock = model.Stock
                    });
                    break;

                    break;

                case "pcpart":
                    _context.PCParts.Add(new PCPart
                    {
                        Category = model.Category,
                        Brand = model.Brand,
                        Model = model.Name,
                        Compatibility = model.Description,
                        Price = model.Price,
                        Stock = model.Stock,
                        ImageURL = imageName,
                        AdditionalImageURLs = string.Join(",", additionalImageUrls)
                    });
                    break;

                    break;

                default:
                    return BadRequest("Invalid type selected.");
            }

            _context.SaveChangesAsync(); // استخدم SaveChangesAsync للتعامل مع العمليات بشكل غير متزامن
            return RedirectToAction("Index");
        }




        //[HttpGet]
        //public IActionResult EditProduct(int id, string type)
        //{
        //    ProductViewModel viewModel = null;

        //    switch (type.ToLower())
        //    {
        //        case "laptop":
        //            var laptop = _context.Laptops.FirstOrDefault(l => l.LaptopID == id);
        //            if (laptop == null) return NotFound();
        //            viewModel = new ProductViewModel
        //            {
        //                ID = laptop.LaptopID,
        //                Name = laptop.Model,
        //                Description = laptop.Description,
        //                Price = laptop.Price,
        //                Brand = laptop.Brand,
        //                Processor = laptop.Processor,
        //                RAM = laptop.RAM,
        //                Storage = laptop.Storage,
        //                GPU = laptop.GPU,
        //                ScreenSize = laptop.ScreenSize,
        //                Stock = laptop.Stock,
        //                ImageURL = laptop.ImageURL,
        //                AdditionalImageURLs = (laptop.AdditionalImageURLs ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
        //                Type = "Laptop"
        //            };
        //            break;

        //        case "pc":
        //            var pc = _context.PCs.FirstOrDefault(p => p.PCID == id);
        //            if (pc == null) return NotFound();
        //            viewModel = new ProductViewModel
        //            {
        //                ID = pc.PCID,
        //                Name = pc.Brand,
        //                Description = pc.Description,
        //                Price = pc.Price,
        //                Brand = pc.Brand,
        //                Processor = pc.Processor,
        //                RAM = pc.RAM,
        //                Storage = pc.Storage,
        //                GPU = pc.GPU,
        //                Stock = pc.Stock,
        //                ImageURL = pc.ImageURL,
        //                AdditionalImageURLs = (pc.AdditionalImageURLs ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
        //                Type = "PC"
        //            };
        //            break;

        //        case "pcpart":
        //            var part = _context.PCParts.FirstOrDefault(p => p.PartID == id);
        //            if (part == null) return NotFound();
        //            viewModel = new ProductViewModel
        //            {
        //                ID = part.PartID,
        //                Name = part.Model,
        //                Description = part.Compatibility,
        //                Price = part.Price,
        //                Brand = part.Brand,
        //                Category = part.Category,
        //                Stock = part.Stock,
        //                ImageURL = part.ImageURL,
        //                AdditionalImageURLs = (part.AdditionalImageURLs ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
        //                Type = "PCPart"
        //            };
        //            break;

        //        default:
        //            return BadRequest("Invalid type.");
        //    }

        //    return View(viewModel);
        //}



        [HttpGet]
        public IActionResult EditProduct(int id, string type)
        {
            ProductViewModel viewModel = null;

            switch (type.ToLower())
            {
                case "laptop":
                    var laptop = _context.Laptops.FirstOrDefault(l => l.LaptopID == id);
                    if (laptop == null) return NotFound();
                    viewModel = new ProductViewModel
                    {
                        ID = laptop.LaptopID,
                        Name = laptop.Model,
                        Description = laptop.Description,
                        Price = laptop.Price,
                        Brand = laptop.Brand,
                        Processor = laptop.Processor,
                        RAM = laptop.RAM,
                        Storage = laptop.Storage,
                        GPU = laptop.GPU,
                        ScreenSize = laptop.ScreenSize,
                        Stock = laptop.Stock,
                        ImageURL = laptop.ImageURL,
                        AdditionalImageURLs = (laptop.AdditionalImageURLs ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                        Type = "Laptop"
                    };
                    break;

                case "pc":
                    var pc = _context.PCs.FirstOrDefault(p => p.PCID == id);
                    if (pc == null) return NotFound();
                    viewModel = new ProductViewModel
                    {
                        ID = pc.PCID,
                        Name = pc.Brand,
                        Description = pc.Description,
                        Price = pc.Price,
                        Brand = pc.Brand,
                        Processor = pc.Processor,
                        RAM = pc.RAM,
                        Storage = pc.Storage,
                        GPU = pc.GPU,
                        Stock = pc.Stock,
                        ImageURL = pc.ImageURL,
                        AdditionalImageURLs = (pc.AdditionalImageURLs ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                        Type = "PC"
                    };
                    break;


                case "pcpart":
                    var part = _context.PCParts.FirstOrDefault(p => p.PartID == id);
                    if (part == null) return NotFound();
                    viewModel = new ProductViewModel
                    {
                        ID = part.PartID,
                        Name = part.Model,
                        Description = part.Compatibility,
                        Price = part.Price,
                        Brand = part.Brand,
                        Category = part.Category,
                        Stock = part.Stock,
                        ImageURL = part.ImageURL,
                        AdditionalImageURLs = (part.AdditionalImageURLs ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                        Type = "PCPart"
                    };

                    // ✅ هنا بنرجع View مخصص إذا كان PCPart
                    return View("EditPCPart", viewModel);
            }

            // الافتراضي: رجّع View العادي
            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductViewModel model, IFormFile ImageFile, IFormFileCollection AdditionalImages)
        {
           

            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string imageName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string imagePath = Path.Combine(uploadsFolder, imageName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                model.ImageURL = imageName;
            }

            // تجهيز صور إضافية
            List<string> newAdditionalImages = new();
            if (AdditionalImages != null && AdditionalImages.Count > 0)
            {
                foreach (var file in AdditionalImages)
                {
                    if (file != null && file.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        newAdditionalImages.Add(fileName);
                    }
                }
            }

            // تعديل حسب نوع المنتج
            switch (model.Type.ToLower())
            {
                case "laptop":
                    var laptop = _context.Laptops.FirstOrDefault(l => l.LaptopID == model.ID);
                    if (laptop == null) return NotFound();

                    laptop.Model = model.Name;
                    laptop.Description = model.Description;
                    laptop.Price = model.Price;
                    laptop.Brand = model.Brand;
                    laptop.Processor = model.Processor;
                    laptop.RAM = model.RAM;
                    laptop.Storage = model.Storage;
                    laptop.GPU = model.GPU;
                    laptop.ScreenSize = model.ScreenSize;
                    laptop.Stock = model.Stock;
                    if (!string.IsNullOrWhiteSpace(model.ImageURL)) laptop.ImageURL = model.ImageURL;
                    if (newAdditionalImages.Any()) laptop.AdditionalImageURLs = string.Join(",", newAdditionalImages);
                    break;

                case "pc":
                    var pc = _context.PCs.FirstOrDefault(p => p.PCID == model.ID);
                    if (pc == null) return NotFound();

                    pc.Brand = model.Brand;
                    pc.Description = model.Description;
                    pc.Price = model.Price;
                    pc.Processor = model.Processor;
                    pc.RAM = model.RAM;
                    pc.Storage = model.Storage;
                    pc.GPU = model.GPU;
                    pc.Stock = model.Stock;
                    if (!string.IsNullOrWhiteSpace(model.ImageURL)) pc.ImageURL = model.ImageURL;
                    if (newAdditionalImages.Any()) pc.AdditionalImageURLs = string.Join(",", newAdditionalImages);
                    break;

                case "pcpart":
                    var part = _context.PCParts.FirstOrDefault(p => p.PartID == model.ID);
                    if (part == null) return NotFound();

                    part.Category = model.Category;
                    part.Model = model.Name;
                    part.Compatibility = model.Description;
                    part.Price = model.Price;
                    part.Brand = model.Brand;
                    part.Stock = model.Stock;
                    if (!string.IsNullOrWhiteSpace(model.ImageURL)) part.ImageURL = model.ImageURL;
                    if (newAdditionalImages.Any()) part.AdditionalImageURLs = string.Join(",", newAdditionalImages);
                    break;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction("Index"); 
        }


        [HttpPost]
        public async Task<IActionResult> DeleteProducts(int id, string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return BadRequest("Product type is required.");

            type = type.ToLower(); 

            object entity = null;

            switch (type)
            {
                case "laptop":
                    entity = await _context.Laptops.FirstOrDefaultAsync(l => l.LaptopID == id);
                    break;

                case "pc":
                    entity = await _context.PCs.FirstOrDefaultAsync(p => p.PCID == id);
                    break;

                case "pcpart":
                    entity = await _context.PCParts.FirstOrDefaultAsync(p => p.PartID == id);
                    break;

                default:
                    return BadRequest("Invalid product type.");
            }

            if (entity == null)
                return NotFound("Product not found.");

            try
            {
                _context.Remove(entity);
                await _context.SaveChangesAsync(); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error occurred while deleting the product: " + ex.Message);
            }

            return RedirectToAction("Index");
        }



    }
}
