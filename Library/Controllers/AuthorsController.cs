using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.Models;
using Library.ViewModels;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Drawing;

namespace Library.Controllers
{
    [Authorize(Roles = "Admin, Editor")]
    public class AuthorsController : Controller
    {
        private readonly AppDbContext _context;

        public AuthorsController(AppDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index(string? id)
        {
            if (!string.IsNullOrEmpty(id)) //take the authors with starting letter
            {               
                return View(_context.Author.Where(p => p.Name.StartsWith(id)).OrderBy(p => p.Name).ToList()); 
            }
            List<Author> popularAuthors = _context.Author
                                                    .GroupJoin( //join with another join
                                                        _context.BookAuthors
                                                            .Join(_context.Rent, //join based on BooksId
                                                                ba => ba.BooksId,
                                                                r => r.BooksId,
                                                                (ba, r) => new { ba.AuthorId } //take the AuthorId
                                                            ),
                                                        a => a.Id, //AuthorId
                                                        x => x.AuthorId, //comes from BookAuthors and Rent join
                                                        (a, x) => new { Author = a, RentCounts = x.Count() } //take the Author row and count
                                                    )
                                                    .OrderByDescending(x => x.RentCounts)
                                                    .Select(x => x.Author)
                                                    .ToList();
            return View(popularAuthors);
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

            var author = await _context.Author.FirstOrDefaultAsync(m => m.Id == id);

            if (author == null)
            {
                return NotFound();
            }
            var booksQuery = _context.Books.Where(a => _context.BookAuthors
                                        .Where(ba => ba.AuthorId == id)
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

            int pageSize = 6; //number of books to show in 1 page
            var paginatedBooks = await PaginatedList<Books>.CreateAsync(booksQuery.AsNoTracking(), pageNumber ?? 1, pageSize);

            author.PaginatedBooks = paginatedBooks;           
            return View(author);
        }

        public IActionResult Create()
        {
            return View();
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAuthorViewModel model)
        {
            if (ModelState.IsValid)
            {
                string filePath = "";
                string imageName = "";
                if (model.Image != null && model.Image.Length > 0)
                {
                    string guid = Guid.NewGuid().ToString(); //For unique names
                    imageName = guid + "." + Path.GetExtension(model.Image.FileName);
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Authors", imageName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await model.Image.CopyToAsync(stream);
                    }
                }
                Author author = new Author
                {
                    Name = model.AuthorName,
                    Description = model.Description
                };
                if(!filePath.IsNullOrEmpty())
                    author.ImagePath = "/images/Authors/" + imageName;
                else //default image
                    author.ImagePath = "/images/Authors/" + "Yazar.png";
                _context.Add(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Author.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            EditAuthorViewModel model = new EditAuthorViewModel { 
                Description = author.Description,
                Id = author.Id,
                Name = author.Name,
                ImagePath = author.ImagePath
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditAuthorViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string filePath = "";
                string imageName = "";
                if (model.NewImage != null && model.NewImage.Length > 0)
                {
                    string guid = Guid.NewGuid().ToString(); //For unique names
                    imageName = guid + "." + Path.GetExtension(model.NewImage.FileName);
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Authors", imageName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await model.NewImage.CopyToAsync(stream);
                    }
                }
                Author author = new Author
                {
                    Id = id,
                    Name = model.Name,
                    Description = model.Description,
                    ImagePath =model.ImagePath,
                };
                if (!filePath.IsNullOrEmpty()) //if previous image exists, change the path
                {
                    author.ImagePath = "/images/Authors/" + imageName;
                    if(!model.ImagePath.IsNullOrEmpty() && (model.ImagePath != "/images/Authors/Yazar.png")) //if it's not the default image delete it
                        System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", model.ImagePath.Substring(1)));
                }
                   
                try
                {
                    _context.Update(author);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.Id))
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
            return View(model);
        }

        public IActionResult Delete(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var author = _context.Author.Find(id);

            if (author == null)
            {
                return NotFound();
            }

            _context.Author.Remove(author);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool AuthorExists(int id)
        {
            return _context.Author.Any(e => e.Id == id);
        }
    }
}
