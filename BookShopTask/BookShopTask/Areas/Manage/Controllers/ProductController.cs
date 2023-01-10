using BookShopTask.DAL;
using BookShopTask.Models;
using BookShopTask.Utilities;
using BookShopTask.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookShopTask.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class ProductController : Controller
    {
        IWebHostEnvironment _env { get; }
        AppDbContext _context { get; }
        public ProductController(IWebHostEnvironment env, AppDbContext context)
        {
            _env = env;
            _context = context;
        }
        public IActionResult Index()
        {
            return View(_context.Products?.Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).Include(p => p.ProductImages).Where(p => p.IsDeleted == false));
        }
        public IActionResult Delete(int? id)
        {
            if (id is null || id == 0) return BadRequest();
            Product exist = _context.Products.Include(p => p.ProductCategories)
                .Include(p => p.ProductTags).Include(p => p.ProductImages).FirstOrDefault(p => p.Id == id);
            if (exist is null) return NotFound();
            foreach (ProductImage image in exist.ProductImages)
            {
                image.ImageUrl.DeleteFile(_env.WebRootPath, "assets/images/product");
            }
            _context.ProductTags.RemoveRange(exist.ProductTags);
            _context.ProductCategories.RemoveRange(exist.ProductCategories);
            _context.ProductImages.RemoveRange(exist.ProductImages);
            exist.IsDeleted = true;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Create()
        {
            ViewBag.Tags = new SelectList(_context.Tags, nameof(Tag.Id), nameof(Tag.Name));
            ViewBag.Categories = new SelectList(_context.Categories, nameof(Category.Id), nameof(Category.Name));
            return View();
        }
        [HttpPost]
        public IActionResult Create(CreateProductVM cp)
        {
            var coverImg = cp.CoverImage;
            var otherImgs = cp.OtherImages ?? new List<IFormFile>();
            string result = coverImg?.CheckValidate("image/", 600);
            if (result?.Length > 0)
            {
                ModelState.AddModelError("Cover Image", result);
            }
            foreach (var image in otherImgs)
            {
                result = image.CheckValidate("image/", 600);
                if (result?.Length > 0)
                {
                    ModelState.AddModelError("OtherImages", result);
                }
            }
            foreach (int tagId in (cp.TagIds ?? new List<int>()))
            {
                if (!_context.Tags.Any(c => c.Id == tagId))
                {
                    ModelState.AddModelError("TagIds", "There is no matched color with this id!");
                    break;
                }
            }
            foreach (int categoryId in (cp.CategoryIds ?? new List<int>()))
            {
                if (!_context.Categories.Any(c => c.Id == categoryId))
                {
                    ModelState.AddModelError("CategoryIds", "There is no matched category with this id!");
                    break;
                }
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Tags = new SelectList(_context.Tags, nameof(Tag.Id), nameof(Tag.Name));
                ViewBag.Categories = new SelectList(_context.Categories, nameof(Category.Id), nameof(Category.Name));
                return View();
            }
            var tags = _context.Tags.Where(s => cp.TagIds.Contains(s.Id));
            var categories = _context.Categories.Where(c => cp.CategoryIds.Contains(c.Id));
            Product product = new Product
            {
                Name = cp.Name,
                CostPrice = cp.CostPrice,
                SellPrice = cp.SellPrice,
                Description = cp.Description,
                Discount = cp.Discount,
                IsDeleted = false,
                SKU = "1235"
            };
            List<ProductImage> images = new List<ProductImage>();
            images.Add(
                new ProductImage
                {
                    ImageUrl = coverImg?.SaveFile(Path.Combine(_env.WebRootPath, "assets", "images", "product")),
                    IsCover = true,
                    Product = product
                });
            foreach (var item in otherImgs)
            {
                images.Add(
                    new ProductImage
                    {
                        ImageUrl = item?.SaveFile(Path.Combine(_env.WebRootPath, "assets", "images", "product")),
                        IsCover = false,
                        Product = product
                    });
            }
            product.ProductImages = images;
            _context.Products.Add(product);
            foreach (var item in tags)
            {
                _context.ProductTags.Add(new ProductTag { Product = product, TagId = item.Id });
            }
            foreach (var item in categories)
            {
                _context.ProductCategories.Add(new ProductCategory { Product = product, CategoryId = item.Id });
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Update(int? id)
        {
            if (id is null) return BadRequest();
            Product product = _context.Products.Find(id);
            if (product is null) return NotFound();
            ViewBag.Tags = new SelectList(_context.Tags, nameof(Tag.Id), nameof(Tag.Name));
            ViewBag.Categories = new SelectList(_context.Categories, nameof(Category.Id), nameof(Category.Name));
            var categories = _context.ProductCategories.Where(pc =>pc.ProductId == product.Id);
            var tags = _context.ProductTags.Where(pc => pc.ProductId == product.Id);
            UpdateProductVM up = new UpdateProductVM();
            up.CategoryIds = new List<int>();
            up.TagIds = new List<int>();
            foreach (var category in categories)
            {
                up.CategoryIds.Add(category.CategoryId);
            }
            foreach (var tag in tags)
            {
                up.TagIds.Add(tag.TagId);
            }
            up.Name = product.Name;
            up.Description = product.Description;
            up.CostPrice = product.CostPrice;
            up.SellPrice = product.SellPrice;
            up.Discount = product.Discount;
            return View(up);
        }
        [HttpPost]
        public IActionResult Update(int? id, UpdateProductVM up)
        {
            if (id is null || id != up.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Tags = new SelectList(_context.Tags, nameof(Tag.Id), nameof(Tag.Name));
                ViewBag.Categories = new SelectList(_context.Categories, nameof(Category.Id), nameof(Category.Name));
                return View();
            }
            Product exist = _context.Products.Find(id);
            if (exist is null) return NotFound();
            exist.Name = up.Name;
            exist.Description = up.Description;
            exist.CostPrice = up.CostPrice;
            exist.SellPrice = up.SellPrice;
            exist.Discount = up.Discount;
            exist.ProductCategories = new List<ProductCategory>();
            ProductCategory pc = new ProductCategory();
            pc.Product = exist;
            foreach (var item in up.CategoryIds)
            {
                pc.CategoryId = item;
                if (_context.ProductCategories.Any(p => p.ProductId == exist.Id && p.CategoryId == pc.CategoryId) == false)
                {
                    exist.ProductCategories.Add(pc);
                }
            }
            exist.ProductTags = new List<ProductTag>();
            ProductTag pt = new ProductTag();
            pt.Product = exist;
            foreach (var item in up.TagIds)
            {
                pt.TagId = item;
                if (_context.ProductTags.Any(p=>p.ProductId == exist.Id && p.TagId ==pt.TagId) == false)
                {
                exist.ProductTags.Add(pt);
                }
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult UpdateImg(int? id)
        {
            if (id is null) return BadRequest();
            Product product = _context.Products.Find(id);
            if (product is null) return NotFound();
            var images = _context.ProductImages.Where(pi => pi.ProductId == product.Id);
            UpdateProductImgVM up = new UpdateProductImgVM();
            up.ProductImages = new List<ProductImage>();
            foreach (var item in images)
            {
                up.ProductImages.Add(item);
            }
            return View(up);
        }
        [HttpPost]
        public IActionResult UpdateImg(int? id, UpdateProductImgVM up)
        {
            if (id is null) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View();
            }
            Product exist = _context.Products.Find(id);
            if (exist is null) return NotFound();
            ViewBag.Images = _context.ProductImages.Where(pi => pi.ProductId == exist.Id).ToList();
            var coverImg = up.CoverImage;
            var otherImgs = up.OtherImages ?? new List<IFormFile>();
            string result = coverImg?.CheckValidate("image/", 600);
            if (result?.Length > 0)
            {
                ModelState.AddModelError("Cover Image", result);
            }
            foreach (var image in otherImgs)
            {
                result = image.CheckValidate("image/", 600);
                if (result?.Length > 0)
                {
                    ModelState.AddModelError("OtherImages", result);
                }
            }
            List<ProductImage> newimages = new List<ProductImage>();
            if (coverImg != null)
            {

            newimages.Add(
                new ProductImage
                {
                    ImageUrl = coverImg?.SaveFile(Path.Combine(_env.WebRootPath, "assets", "images", "product")),
                    IsCover = true,
                    Product = exist
                });
            }
            foreach (var item in otherImgs)
            {
                newimages.Add(
                    new ProductImage
                    {
                        ImageUrl = item?.SaveFile(Path.Combine(_env.WebRootPath, "assets", "images", "product")),
                        IsCover = false,
                        Product = exist
                    });
            }
            if (newimages.Any(pi=>pi.IsCover == true))
            {
            foreach (ProductImage image in ViewBag.Images)
            {
                if (image.IsCover == true)
                {
                        image.ImageUrl.DeleteFile(_env.WebRootPath, "assets/images/product");
                        _context.ProductImages.Remove(image);
                    }
                }
            }
            if (newimages.Any(pi=>pi.IsCover == false))
            {
                foreach (ProductImage image in exist.ProductImages)
                {
                    if (image.IsCover == false)
                    {
                        image.ImageUrl.DeleteFile(_env.WebRootPath, "assets/images/product");
                        _context.ProductImages.Remove(image);
                    }
                }
            }
            foreach (var item in newimages)
            {
            exist.ProductImages.Add(item);
            };
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
