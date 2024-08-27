using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Data.SqlClient;

namespace Library.Controllers
{
    [Authorize(Roles = "Admin, Editor")]
    public class TranslatorsController : Controller
    {
        private readonly AppDbContext _context;

        public TranslatorsController(AppDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? id)
        {
            if (!string.IsNullOrEmpty(id)) //take the translators with starting letter
            {              
                return View(await _context.Translator.Where(p => p.Name.StartsWith(id)).OrderBy(p => p.Name).ToListAsync());
            }
            List<Translator> popularTranslators = _context.Translator
                                                    .GroupJoin(//join with another join
                                                        _context.BookTranslator
                                                            .Join(_context.Rent,//join based on BooksId
                                                                ba => ba.BooksId,
                                                                r => r.BooksId,
                                                                (ba, r) => new { ba.TranslatorId }//take the TranslatorId
                                                            ),
                                                        a => a.Id,//TranslatorId
                                                        x => x.TranslatorId, //comes from BookTranslator and Rent join
                                                        (a, b) => new { Translator = a, RentCounts = b.Count() }//take the Translator row and count
                                                    )
                                                    .OrderByDescending(x => x.RentCounts)
                                                    .Select(x => x.Translator)
                                                    .ToList();
            return View(popularTranslators);
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

            var translator = await _context.Translator
                .FirstOrDefaultAsync(m => m.Id == id);
            if (translator == null)
            {
                return NotFound();
            }
            var booksQuery = _context.Books
                     .Where(a => _context.BookTranslator
                                        .Where(ba => ba.TranslatorId == id)
                                        .Select(ba => ba.BooksId)
                                        .Contains(a.Id));
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

            int pageSize = 10;//number of books to show in 1 page
            var paginatedBooks = await PaginatedList<Books>.CreateAsync(booksQuery.AsNoTracking(), pageNumber ?? 1, pageSize);

            translator.PaginatedBooks = paginatedBooks;
            return View(translator);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Translator translator)
        {
            if (!String.IsNullOrEmpty(translator.Name))
            {
                _context.Add(translator);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(translator);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var translator = await _context.Translator.FindAsync(id);
            if (translator == null)
            {
                return NotFound();
            }
            return View(translator);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Translator translator)
        {
            if (id != translator.Id)
            {
                return NotFound();
            }

            if (!String.IsNullOrEmpty(translator.Name))
            {
                try
                {
                    _context.Update(translator);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TranslatorExists(translator.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { id = id });
            }
            return View(translator);
        }

        public IActionResult Delete(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var translator = _context.Translator.Find(id);

            if (translator == null)
            {
                return NotFound();
            }

            _context.Translator.Remove(translator);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool TranslatorExists(int id)
        {
            return _context.Translator.Any(e => e.Id == id);
        }
    }
}
