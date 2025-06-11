//using Master_pice.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Master_pice.Controllers
//{
//    public class FavoriteController : Controller
//    {
//        private readonly AppDbContext _context;

//        public FavoriteController(AppDbContext context)
//        {
//            _context = context;
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult AddToFavorites(int productId, string type)
//        {
//            int? userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null)
//                return RedirectToAction("Login", "Authintication");

//            bool exists = _context.Favorites.Any(f =>
//                f.UserID == userId &&
//                ((type == "laptop" && f.LaptopID == productId) ||
//                 (type == "pc" && f.PCID == productId) ||
//                 (type == "pcpart" && f.PartID == productId)));

//            if (!exists)
//            {
//                var favorite = new Favorite
//                {
//                    UserID = userId.Value,
//                    LaptopID = null,
//                    PCID = null,
//                    PartID = null
//                };

//                if (type == "laptop")
//                {
//                    var laptop = _context.Laptops.FirstOrDefault(l => l.LaptopID == productId);
//                    if (laptop == null) return NotFound();
//                    favorite.LaptopID = productId;
//                }
//                else if (type == "pc")
//                {
//                    var pc = _context.PCs.FirstOrDefault(p => p.PCID == productId);
//                    if (pc == null) return NotFound();
//                    favorite.PCID = productId;
//                }
//                else if (type == "pcpart")
//                {
//                    var part = _context.PCParts.FirstOrDefault(p => p.PartID == productId);
//                    if (part == null) return NotFound();
//                    favorite.PartID = productId;
//                }


//                _context.Favorites.Add(favorite);
//                _context.SaveChanges();
//            }

//            TempData["SuccessMessage"] = "Added to Favorites!";
//            return RedirectToAction("Details", "Product", new { id = productId, type = type });
//        }



//        public IActionResult MyFavorites()
//        {
//            int? userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null)
//                return RedirectToAction("Login", "Authintication");

//            var favorites = _context.Favorites.Where(f => f.UserID == userId).ToList();

//            var favoriteItems = new List<dynamic>();

//            foreach (var fav in favorites)
//            {
//                string name = "";
//                string image = "";
//                string type = "";

//                if (fav.LaptopID != null)
//                {
//                    var laptop = _context.Laptops.FirstOrDefault(l => l.LaptopID == fav.LaptopID);
//                    if (laptop != null)
//                    {
//                        name = laptop.Brand + " " + laptop.Model;
//                        image = laptop.ImageURL;
//                        type = "laptop";
//                    }
//                }
//                else if (fav.PCID != null)
//                {
//                    var pc = _context.PCs.FirstOrDefault(p => p.PCID == fav.PCID);
//                    if (pc != null)
//                    {
//                        name = pc.Brand + " " + pc.Processor;
//                        image = pc.ImageURL;
//                        type = "pc";
//                    }
//                }
//                else if (fav.PartID != null)
//                {
//                    var part = _context.PCParts.FirstOrDefault(p => p.PartID == fav.PartID);
//                    if (part != null)
//                    {
//                        name = part.Model;
//                        image = part.ImageURL;
//                        type = "pcpart";
//                    }
//                }

//                favoriteItems.Add(new
//                {
//                    ProductId = fav.LaptopID ?? fav.PCID ?? fav.PartID,
//                    Name = name,
//                    Image = image,
//                    Type = type
//                });
//            }

//            return View(favoriteItems);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult RemoveFavorite(int id, string type)
//        {
//            int? userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null)
//                return RedirectToAction("Login", "Authintication");

//            var favorite = _context.Favorites.FirstOrDefault(f =>
//                f.UserID == userId &&
//                ((type == "laptop" && f.LaptopID == id) ||
//                 (type == "pc" && f.PCID == id) ||
//                 (type == "pcpart" && f.PartID == id)));

//            if (favorite != null)
//            {
//                _context.Favorites.Remove(favorite);
//                _context.SaveChanges();
//            }

//            TempData["SuccessMessage"] = "Removed from Favorites!";
//            return RedirectToAction("MyFavorites");
//        }
//    }

//}
