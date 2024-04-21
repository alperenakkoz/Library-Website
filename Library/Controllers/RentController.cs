using Library.Models;
using Library.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Data;
using System.Linq;
using static System.Reflection.Metadata.BlobBuilder;

namespace Library.Controllers
{
    [Authorize(Roles = "Admin, Editor")]
    public class RentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<LibraryUser> _userManager;
        private readonly IStringLocalizer<RentController> _localizer;

        public RentController(AppDbContext context, UserManager<LibraryUser> userManager, IStringLocalizer<RentController> localizer)
        {
            _context = context;
            _userManager = userManager;
            _localizer = localizer;
        }
        public IActionResult Index()
        {                     
            var model = new List<UserViewModel>();
            foreach (var user in _userManager.Users.ToList())
            {
                var UserViewModel = new UserViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                };
                model.Add(UserViewModel);
            }
                       
            return View(model);
        }
        
        //rentbook page
        public async Task<IActionResult> RentBook(string? warning, string sortOrder, string currentFilter, string searchString, int? pageNumber, string? id)
        {
            if (id == null)
                return NotFound();

            if (!_userManager.Users.Any(x => x.Id == id))
                return NotFound();
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var booksQuery = from b in _context.Books select b;
            if (!String.IsNullOrEmpty(searchString))
            {
                booksQuery = booksQuery.Where(b => b.Title.Contains(searchString));
            }
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

            List<Books> reservedBooks = _context.Books.Where(x => _context.Hold
                                                .Where(h => h.LibraryUserId == id)
                                                .Select(h => h.BooksId)
                                                .Contains(x.Id)).ToList();

            List<int> reservedBookIds = _context.Hold.Where(h => h.LibraryUserId == id) //for removing from rentable books
                                                .Select(h => h.BooksId)
                                                .ToList();

            var today = DateTime.Today;
            List<Books> rentedBooks = _context.Books.Where(x => _context.Rent
                                                .Where(h => h.LibraryUserId == id && h.IsReturned == false && h.EndTime.Date >= today.Date)
                                                .Select(h => h.BooksId)
                                                .Contains(x.Id)).ToList();
          
            List<Books> OverdueBooks = _context.Books.Where(x => _context.Rent
                                                .Where(h => h.LibraryUserId == id && h.IsReturned == false && h.EndTime.Date < today.Date)
                                                .Select(h => h.BooksId)
                                                .Contains(x.Id)).ToList();
            //for removing from rentable books
            List<int> rentBookIds = _context.Rent.Where(x => x.LibraryUserId == id && x.IsReturned == false).Select(x => x.BooksId).ToList(); 

            booksQuery = booksQuery.Where(x => !reservedBookIds.Contains(x.Id) && !rentBookIds.Contains(x.Id) && x.numberOfBooks > 0);
            int pageSize = 10; //number of books shown in rentable books
            var paginatedBooks = await PaginatedList<Books>.CreateAsync(booksQuery.AsNoTracking(), pageNumber ?? 1, pageSize);
            UserViewModel userViewModel = new UserViewModel
            {
                UserId = id,
                PaginatedBooks = paginatedBooks,
                ReservedBooks = reservedBooks,
                RentedBooks = rentedBooks,
                OverdueBooks = OverdueBooks,
            };
            ViewData["Warning"] = warning; //warning for number of books and overdue books
            return View(userViewModel);
        }

        [HttpPost]
        public IActionResult RentBook(string UserId, string[] image) //AJAX
        {
            if(image.Length > 4)
            {//max rentable book is 4
                return RedirectToAction("RentBook", new { warning = _localizer["Limit4"] });
            }
            //make it int
            List<int> rentedBookIds = image.Select(int.Parse).ToList();
            //if it not in array than it's returned 
            var returnedBooks = _context.Rent .Where(x => x.LibraryUserId == UserId && !x.IsReturned && !rentedBookIds.Contains(x.BooksId)).ToList();

            foreach (var rentedBook in returnedBooks)
            {
                rentedBook.IsReturned = true;
                var book = _context.Books.FirstOrDefault(x => x.Id == rentedBook.BooksId);
                book.numberOfBooks++;
                
                WaitList waitListUser = _context.WaitList.OrderBy(x => x.CreateDate).FirstOrDefault(x => x.BooksId == rentedBook.BooksId);
                if(waitListUser != null) //if waitlist not null give the returned book to them
                {
                    Hold holdUser = new Hold
                    {
                        LibraryUserId = waitListUser.LibraryUserId,
                        BooksId = waitListUser.BooksId,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now.AddDays(2)
                    };
                    _context.Add(holdUser);
                    _context.Remove(waitListUser);
                    book.numberOfBooks--;
                }              
                _context.Update(book);
            }
            //i just could delete all but it would reset the date.
            List<int> rentBookIdsInDb = _context.Rent.Where(x => x.LibraryUserId == UserId && !x.IsReturned && rentedBookIds.Contains(x.BooksId)).Select(x=> x.BooksId).ToList();
            bool hasOverDue = _context.Rent.Where(x => x.LibraryUserId == UserId && !x.IsReturned && x.EndTime.Date < DateTime.Now.Date && rentedBookIds.Contains(x.BooksId)).Any();
            foreach (int bookId  in rentedBookIds)
            {              
                if (!rentBookIdsInDb.Contains(bookId)){
                    if (hasOverDue)
                    {
                        return RedirectToAction("RentBook", new { warning = _localizer["OverDueWarning"] });
                    }
                    Rent rent = new Rent
                    {
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now.AddDays(15),
                        LibraryUserId = UserId,
                        BooksId = bookId,
                        IsReturned = false
                    };
                    _context.Add(rent);
                                      
                    var reserve = _context.Hold.FirstOrDefault(x => x.LibraryUserId == UserId && x.BooksId == bookId);
                    if(reserve != null)
                    {
                        _context.Remove(reserve);
                    }
                    else
                    {
                        var book = _context.Books.FirstOrDefault(x => x.Id == bookId);
                        book.numberOfBooks--;
                        _context.Update(book);
                   }                    
                }               
               
            }
            _context.SaveChanges();
            return RedirectToAction("RentBook");
        }
    }
}
