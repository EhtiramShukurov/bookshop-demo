using BookShopTask.DAL;
using BookShopTask.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookShopTask.Areas.Manage.Controllers
{
    [Area("Manage")]

    public class TagController : Controller
    {
        AppDbContext _context { get; }
        public TagController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View(_context.Tags.ToList());
        }
        public IActionResult Delete(int id)
        {
            Tag tag = _context.Tags.Find(id);
            if (tag is null) return NotFound();
            _context.Tags.Remove(tag);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            _context.Tags.Add(tag);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Update(int? id)
        {
            if (id is null) return BadRequest();
            Tag Tag = _context.Tags.Find(id);
            if (Tag is null) return NotFound();
            return View(Tag);
        }
        [HttpPost]
        public IActionResult Update(int? id, Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (id is null || id != tag.Id) return BadRequest();
            Tag exist = _context.Tags.Find(id);
            if (exist is null) return NotFound();
            exist.Name = tag.Name;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
