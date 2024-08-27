using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.Models;
using Microsoft.AspNetCore.Authorization;
using Library.ViewModels;
using static System.Reflection.Metadata.BlobBuilder;
using System.Collections;

namespace Library.Controllers
{
    [Authorize(Roles = "Admin, Editor")]
    public class PublishersController : Controller
    {
        private readonly AppDbContext _context;

        public PublishersController(AppDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index(string? id)
        {
            if (!string.IsNullOrEmpty(id)) //take the publishers with starting letter
            {              
                return View(_context.Publisher.Where(p => p.Name.StartsWith(id)).OrderBy(p => p.Name).ToList());                           
            }
            List<Publisher> popularPublishers = _context.Publisher
                                                    .GroupJoin( //join with another join
                                                        _context.Books
                                                            .Join(_context.Rent, //join based on BooksId
                                                                b => b.Id,
                                                                r => r.BooksId,
                                                                (b, r) => new { b.PublisherId }//take the PublisherId
                                                            ),
                                                        a => a.Id, //PublisherId
                                                        x => x.PublisherId, //comes from Books and Rent join
                                                        (a, b) => new { Publisher = a, RentCounts = b.Count() } //take the Publisher row and count
                                                    )
                                                    .OrderByDescending(x => x.RentCounts)
                                                    .Select(x => x.Publisher)
                                                    .ToList();
            return View(popularPublishers);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string sortOrder, int? pageNumber, int? id)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publisher.FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null)
            {
                return NotFound();
            }
            var booksQuery = _context.Books.Where(x => x.PublisherId == id);
            switch (sortOrder)
            {
                case "name_desc":
                    booksQuery = booksQuery.OrderByDescending(b => b.Title);
                    break;
                case "Date":
                    booksQuery = booksQuery.OrderBy(b => b.PublicationDate);
                    break;
                case "date_desc":
                    booksQuery = booksQuery.OrderByDescending(b => b.PublicationDate);
                    break;
                default:
                    booksQuery = booksQuery.OrderBy(b => b.Title);
                    break;
            }

            int pageSize = 10; //number of books to show in 1 page
            var paginatedBooks = await PaginatedList<Books>.CreateAsync(booksQuery.AsNoTracking(), pageNumber ?? 1, pageSize);

            publisher.PaginatedBooks = paginatedBooks;
            return View(publisher);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePublisherViewModel publisher)
        {
            if (ModelState.IsValid)
            {
                _context.Add(new Publisher { Name = publisher.Name});
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(publisher);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publisher.FindAsync(id);
            if (publisher == null)
            {
                return NotFound();
            }
            EditPublisherViewModel model = new () { Name = publisher.Name, Id = (int)id };
            return View(model);
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] EditPublisherViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(new Publisher { Id = id, Name=model.Name});
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublisherExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { id });
            }
            return View(model);
        }

        public IActionResult Delete(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var publisher = _context.Publisher.Find(id);

            if (publisher == null)
            {
                return NotFound();
            }

            _context.Publisher.Remove(publisher);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool PublisherExists(int id)
        {
            return _context.Publisher.Any(e => e.Id == id);
        }
    }
}
