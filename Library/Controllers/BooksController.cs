using Azure;
using Library.Models;
using Library.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;


namespace Library.Controllers
{

    public class BooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<LibraryUser> _userManager;
        private readonly IStringLocalizer<BooksController> _localizer;

        public BooksController(UserManager<LibraryUser> userManager, AppDbContext context, IStringLocalizer<BooksController> localizer)
        {
            _context = context;
            _userManager = userManager;
            _localizer = localizer;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber,
                                                int? languageId, int? categoryId, bool? showOutOfStock)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CheckParm"] = showOutOfStock;

            int selectedCategory = 0;
            var categories = await _context.Category.ToListAsync();
            categories.Insert(0, new Category { Id = 0, Name = _localizer["SelectCategory"] }); //category placeholder
            if (categoryId != null) selectedCategory = (int)categoryId;
            ViewBag.CategorySelect = new SelectList(categories, "Id", "Name", selectedCategory);

            int selectedLanguage = 0;
            var languages = await _context.BookLanguage.ToListAsync();
            languages.Insert(0, new BookLanguage { Id = 0, Name = _localizer["SelectLanguage"] }); //language placeholder
            if (languageId != null) selectedLanguage = (int)languageId;
            ViewBag.BookLanguageSelect = new SelectList(languages, "Id", "Name", selectedLanguage);

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var books = from b in _context.Books select b;
            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    books = books.OrderByDescending(b => b.Title);
                    break;
                case "Date":
                    books = books.OrderBy(b => b.PublicationDate);
                    break;
                case "date_desc":
                    books = books.OrderByDescending(b => b.PublicationDate);
                    break;
                default:
                    books = books.OrderBy(b => b.Title);
                    break;
            }

            if (showOutOfStock == null) {
                books = books.Where(x => x.numberOfBooks > 0);
            }
            if (languageId != null && languageId != 0)
            {
                books = books.Where(x => x.BookLanguageId == languageId);
            }
            if (categoryId != null && categoryId != 0)
            {
                books = books.Where(x => x.CategoryId == categoryId);
            }
            List<int> top5BooksIdRent = await _context.Rent
                .Where(x => x.StartTime >= DateTime.Today.AddDays(-30)) //last 30 days
                .GroupBy(x => x.BooksId) 
                .OrderByDescending(g => g.Count()) 
                .Select(g => g.Key) //take bookid
                .Take(5) 
                .ToListAsync();

            List<int> top5BooksIdHoldandWait = await (
                                from w in _context.WaitList
                                group w by w.BooksId into g
                                select new { BooksId = g.Key, Count = g.Count() }
                            )
                            .Union( //take all records from both of them 
                                from h in _context.Hold
                                group h by h.BooksId into g
                                select new { BooksId = g.Key, Count = g.Count() }
                            )
                            .OrderByDescending(x => x.Count)
                            .Select(x => x.BooksId)
                            .Take(5)
                            .ToListAsync();
            if(top5BooksIdRent.Count < 5) //if not enough books in rent add newest books
            {
                top5BooksIdRent.AddRange(await _context.Books.OrderByDescending(x => x.PublicationDate).Select(x => x.Id).Take(5 - top5BooksIdRent.Count).ToListAsync());

            }
            if(top5BooksIdHoldandWait.Count < 5) //if not enough books in hold and wait add from rent
            {
                top5BooksIdHoldandWait.AddRange(top5BooksIdRent.Take(5 - top5BooksIdHoldandWait.Count));
            }
            ViewData["RentPopular"] = await _context.Books.Where(b => top5BooksIdRent.Contains(b.Id)).ToListAsync();
            ViewData["HighOnDemand"] = await _context.Books.Where(b => top5BooksIdHoldandWait.Contains(b.Id)).ToListAsync();
            ViewData["LatestBooks"] = await _context.Books.OrderByDescending(x => x.PublicationDate).Take(5).ToListAsync();

            string lastVisitedBooks = Request.Cookies["LastVisitedBooks"] ?? ""; //last visited cookie
            string[] bookIds = lastVisitedBooks.Split(',', StringSplitOptions.RemoveEmptyEntries); //1,2,3
            Array.Reverse(bookIds); //latest first
            List<Books> lastVisitedBookDetails = [];
            foreach (string id in bookIds)
            {
                if (int.TryParse(id, out int bookId)) //although it's string in cookie, it's int in db 
                {
                    var book = await _context.Books.FindAsync(bookId);
                    if (book != null)
                    {
                        lastVisitedBookDetails.Add(book);
                    }
                }
            }
            if (lastVisitedBookDetails.Count != 0)
            {
                ViewData["LastVisitedBooks"] = lastVisitedBookDetails;
            }
            else
            {
                ViewData["LastVisitedBooks"] = null;
            }
            

            int pageSize = 10; //10 books shown
            return View(await PaginatedList<Books>.CreateAsync(books.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [Authorize(Roles = "Admin, Editor")]
        public async Task<IActionResult> CreateAsync() {
            ViewBag.AuthorSelect = new SelectList(await _context.Author.ToListAsync(), "Id", "Name");
            ViewBag.BookLanguageSelect = new SelectList(await _context.BookLanguage.ToListAsync(), "Id", "Name");
            ViewBag.PublisherSelect = new SelectList(await _context.Publisher.ToListAsync(), "Id", "Name");
            ViewBag.CategorySelect = new SelectList(await _context.Category.ToListAsync(), "Id", "Name");
            ViewBag.TranslatorSelect = new SelectList(await _context.Translator.ToListAsync(), "Id", "Name");
            return View();
        }

        [Authorize(Roles = "Admin, Editor")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateBookViewModel model) {
            if (ModelState.IsValid)
            {
                Books book = new () {
                    Title = model.Title,
                    ISBN10 = model.ISBN10,
                    ISBN13 = model.ISBN13,
                    NumberOfPages = model.NumberOfPages,
                    PublicationDate = model.PublicationDate,
                    BookLanguageId = model.BookLanguageId,
                    PublisherId = model.PublisherId,
                    CategoryId = model.CategoryId,
                    Description = model.Description,
                    numberOfBooks = model.numberOfBooks
                };
                string filePath = "";
                string imageName = "";
                if (model.Image != null && model.Image.Length > 0)
                {
                    string guid = Guid.NewGuid().ToString(); //unique name
                    imageName = guid + "." + Path.GetExtension(model.Image.FileName);
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Books", imageName);
                    using var stream = System.IO.File.Create(filePath);
                    await model.Image.CopyToAsync(stream);
                }
                if (!String.IsNullOrEmpty(filePath))
                    book.ImagePath = "/images/Books/" + imageName;
                else
                    book.ImagePath = "/images/Books/" + "Kitap.png"; //default image
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                int bookId = (await _context.Books.OrderByDescending(b => b.Id).FirstOrDefaultAsync()).Id;
                for (int i = 0; i < model.AuthorIds.Count; i++) //add connection by using BookAuthors
                {
                    BookAuthors bookAuthor = new () { AuthorId = model.AuthorIds[i], BooksId = bookId };
                    _context.Add(bookAuthor);
                }
                if (model.TranslatorIds != null && model.TranslatorIds.Count != 0)
                {
                    for (int i = 0; i < model.TranslatorIds.Count; i++) //add connection by using BookTranslator
                    {
                        BookTranslator bookTranslator = new () { TranslatorId = model.TranslatorIds[i], BooksId = bookId };
                        _context.Add(bookTranslator);
                    }
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [Authorize(Roles = "Admin, Editor")]
        public async Task<IActionResult> Delete(int? id) { 

            if (id == null) { 
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, Editor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var book = await _context.Books.FindAsync(id);          
            if (book == null) { return NotFound(); }
            List<int> authorIds = await _context.BookAuthors.Where(x => x.BooksId == id).Select(x => x.AuthorId).ToListAsync();
            List<int> translatorIds = await _context.BookTranslator.Where(x => x.BooksId == id).Select(x => x.TranslatorId).ToListAsync();
            ViewBag.AuthorSelect = new SelectList(await _context.Author.ToListAsync(), "Id", "Name", authorIds); //fourth one is default value
            ViewBag.BookLanguageSelect = new SelectList(await _context.BookLanguage.ToListAsync(), "Id", "Name", book.BookLanguageId);
            ViewBag.PublisherSelect = new SelectList(await _context.Publisher.ToListAsync(), "Id", "Name", book.PublisherId);
            ViewBag.CategorySelect = new SelectList(await _context.Category.ToListAsync(), "Id", "Name", book.CategoryId);
            ViewBag.TranslatorSelect = new SelectList(await _context.Translator.ToListAsync(), "Id", "Name", translatorIds);
            EditBookViewModel model = new ()
            {
                Title = book.Title,
                ISBN10 = book.ISBN10,
                ISBN13 = book.ISBN13,
                NumberOfPages = book.NumberOfPages,
                PublicationDate = book.PublicationDate,
                BookLanguageId = book.BookLanguageId,
                PublisherId = book.PublisherId,
                CategoryId = book.CategoryId,
                Description = book.Description,
                AuthorIds = authorIds,
                TranslatorIds = translatorIds,
                ImagePath = book.ImagePath,
                numberOfBooks = book.numberOfBooks
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Editor")]
        public async Task<IActionResult> EditAsync(int id, EditBookViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                Books book = new ()
                {
                    Id = model.Id,
                    Title = model.Title,
                    ISBN10 = model.ISBN10,
                    ISBN13 = model.ISBN13,
                    NumberOfPages = model.NumberOfPages,
                    PublicationDate = model.PublicationDate,
                    BookLanguageId = model.BookLanguageId,
                    PublisherId = model.PublisherId,
                    CategoryId = model.CategoryId,
                    Description = model.Description,
                    ImagePath =model.ImagePath,
                    numberOfBooks = model.numberOfBooks
                };
                int prevBookCount = await _context.Books.Where(x => x.Id == id).Select(x => x.numberOfBooks).FirstOrDefaultAsync();
                int numberOfAddedBooks = book.numberOfBooks - prevBookCount;
                if (numberOfAddedBooks > 0) //if new books are added, we need to give them to waitlist
                {
                    for(int i = 0; i < numberOfAddedBooks; i++)
                    {
                        WaitList? waitListUser = await _context.WaitList.OrderBy(x => x.CreateDate).FirstOrDefaultAsync(x => x.BooksId == id);
                        if (waitListUser != null)
                        {
                            Hold holdUser = new ()
                            {
                                LibraryUserId = waitListUser.LibraryUserId,
                                BooksId = waitListUser.BooksId,
                                StartTime = DateTime.Now,
                                EndTime = DateTime.Now.AddDays(2)
                            };
                            _context.Hold.Add(holdUser);
                            _context.WaitList.Remove(waitListUser);
                            book.numberOfBooks--;
                        }
                        else
                        {
                            break; //if there is no waiting user anymore, exit
                        }                   
                    }
                }

                string filePath = "";
                string imageName = "";
                if (model.NewImage != null && model.NewImage.Length > 0)
                {
                    string guid = Guid.NewGuid().ToString(); //unique name
                    imageName = guid + "." + Path.GetExtension(model.NewImage.FileName);
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Books", imageName);
                    using var stream = System.IO.File.Create(filePath);
                    await model.NewImage.CopyToAsync(stream);
                }
                if (!string.IsNullOrEmpty(filePath))
                {
                    book.ImagePath = "/images/Books/" + imageName; //new image
                    if (!String.IsNullOrEmpty(model.ImagePath) && (model.ImagePath != "/images/Books/Kitap.png")) //delete if it's not default image
                        System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", model.ImagePath[1..]));
                }
                _context.BookAuthors.RemoveRange(_context.BookAuthors.Where(x => x.BooksId == book.Id));
                for (int i = 0; i < model.AuthorIds.Count; i++) //For the changes; just delete all then add the ones come from form
                {
                    BookAuthors bookAuthor = new () { AuthorId = model.AuthorIds[i], BooksId = book.Id };                   
                    _context.Add(bookAuthor);                                                          
                }
                _context.BookTranslator.RemoveRange(_context.BookTranslator.Where(x => x.BooksId == book.Id));
                if (model.TranslatorIds != null && model.TranslatorIds.Count != 0) //For the changes; just delete all then add the ones come from form
                {                   
                    for (int i = 0; i < model.TranslatorIds.Count; i++)
                    {
                        BookTranslator bookTranslator = new () { TranslatorId = model.TranslatorIds[i], BooksId = book.Id };
                        _context.Add(bookTranslator);                      
                    }
                }
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new {id});
            }
            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

        public async Task<IActionResult> Details(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            var cookieOptions = new CookieOptions
            {   
                Expires = DateTime.Now.AddDays(3)
            };
            string lastVisitedBooks = Request.Cookies["LastVisitedBooks"] ?? ""; //1,2,3 ...
            if (!lastVisitedBooks.Contains(id.ToString())) //add new book id to cookie if not already exists
            {
                string[] bookIds = lastVisitedBooks.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (bookIds.Length >= 5) //max number of books is 5
                {
                    lastVisitedBooks = string.Join(",", bookIds.Skip(1)); //skip the first value
                }
                lastVisitedBooks += "," + id;
            }
            Response.Cookies.Append("LastVisitedBooks", lastVisitedBooks, cookieOptions); //set cookie

            List<Comments> comments = await _context.Comments.Where(x => x.BooksId == id).OrderByDescending(x => x.UpdateDate ?? x.CreateDate).ToListAsync();
            List<CommentViewModel> commentViewModels = [];
            ViewData["ChangeComment"] = null;
            string? currentUserId = _userManager.GetUserId(User);
            foreach(var item in comments)
            {
                var user = await _userManager.FindByIdAsync(item.LibraryUserId);
                if(user.Id == currentUserId) { //current user comment
                    ViewData["ChangeComment"] = new ChangeCommentViewModel { CommentId = item.Id, Comment = item.Comment, BooksId = item.BooksId };
                }
                commentViewModels.Add(new CommentViewModel { 
                    Comment = item.Comment, 
                    CreateDate = item.CreateDate, 
                    UpdateDate = item.UpdateDate,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                });
            }
            book.CommentViewModel = commentViewModels;

            book.Authors = await _context.Author
                     .Where(a => _context.BookAuthors
                                        .Where(ba => ba.BooksId == id)
                                        .Select(ba => ba.AuthorId)
                                        .Contains(a.Id))
                     .ToListAsync();
            book.Translators = await _context.Translator
                     .Where(a => _context.BookTranslator
                                        .Where(ba => ba.BooksId == id)
                                        .Select(ba => ba.TranslatorId)
                                        .Contains(a.Id))
                     .ToListAsync();            
            book.Category = await _context.Category.FirstOrDefaultAsync(x => x.Id == book.CategoryId);
            book.Publisher = await _context.Publisher.FirstOrDefaultAsync(x => x.Id == book.PublisherId);

            string userId = _userManager.GetUserId(User); 
            int place = (await _context.WaitList.OrderBy(x => x.CreateDate)
                              .Where(x => x.BooksId == id)
                              .ToListAsync())
                              .IndexOf(await _context.WaitList.FirstOrDefaultAsync(x => x.LibraryUserId == userId && x.BooksId == id)) + 1;

            
            if (place == 0)
            {
                ViewData["resultText"] = null;
            }
            else
            {
                ViewData["resultText"] = PlaceText(place);
            }
           

            if (IsInHoldList((int)id))
                ViewData["ButtonText"] = _localizer["CancelTheReservation"];
            else if (IsInWaitlist((int)id))
                ViewData["ButtonText"] = _localizer["RemoveFromWaitlist"];
            else
                ViewData["ButtonText"] = _localizer["Reserve"];
            //book.Description = book.Description.Replace("\r\n", "<br />").Replace("\n", "<br />");
            return View(book);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Reserve(int id) //AJAX
        {
            string userId = _userManager.GetUserId(User); 
            bool isInWait = IsInWaitlist(id);
            bool isInHold= IsInHoldList(id);
            string buttonText = "";
            string resultText = "";
            if (isInHold) //check hold first. If it's in it remove
            {
                _context.Hold.Remove(await _context.Hold.FirstOrDefaultAsync(x => x.LibraryUserId == userId && x.BooksId == id));
                buttonText = _localizer["Reserve"];
                resultText = _localizer["Cancel"];
                WaitList waitListUser = await _context.WaitList.OrderBy(x => x.CreateDate).FirstOrDefaultAsync(x => x.BooksId == id);

                if (waitListUser != null) //give the book first record from waitlist
                {
                    Hold holdUser = new ()
                    {
                        LibraryUserId = waitListUser.LibraryUserId,
                        BooksId = waitListUser.BooksId,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now.AddDays(2)
                    };
                    _context.Hold.Add(holdUser);
                    _context.WaitList.Remove(waitListUser);
                }
                else
                {
                    var book = await _context.Books.FirstOrDefaultAsync(x=> x.Id == id);
                    book.numberOfBooks++;
                    _context.Books.Update(book);
                }
            }
            else //not in hold
            {
                
                if((await _context.Books.FirstOrDefaultAsync(x => x.Id == id)).numberOfBooks > 0 ) //if books number bigger 0,then no one is in waitlist and you can reserve
                {
                    if (await _context.Hold.CountAsync(x => x.LibraryUserId == userId) + await _context.WaitList.CountAsync(x => x.LibraryUserId == userId) >= 4)
                    {//max number of reserve is 4 
                        return BadRequest(_localizer["Limit4"]);
                    }
                    _context.Hold.Add(new Hold
                    {
                        BooksId = id,
                        LibraryUserId = userId,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now.AddDays(2),
                    });                  
                    var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id);
                    book.numberOfBooks--;
                    _context.Update(book);
                    buttonText = _localizer["CancelTheReservation"];
                    resultText = _localizer["ReservedTheBook"];
                }
                else if (isInWait)
                {
                    _context.WaitList.Remove(await _context.WaitList.FirstOrDefaultAsync(x => x.LibraryUserId == userId && x.BooksId == id));
                    buttonText = _localizer["Reserve"];
                    resultText = _localizer["Cancel"];
                }
                else //add to waitlist
                {
                    if (await _context.Hold.CountAsync(x => x.LibraryUserId == userId) + await _context.WaitList.CountAsync(x => x.LibraryUserId == userId) >= 4)
                    {//max number of reserve is 4 
                        return BadRequest(_localizer["Limit4"]);
                    }
                    _context.WaitList.Add(new WaitList
                    {
                        BooksId = id,
                        LibraryUserId = userId,
                        CreateDate = DateTime.Now,
                    });
                    buttonText = _localizer["RemoveFromWaitlist"];
                    int place = (await _context.WaitList.OrderBy(x => x.CreateDate)
                              .Where(x => x.BooksId == id)
                              .ToListAsync())
                              .IndexOf(await _context.WaitList.FirstOrDefaultAsync(x => x.LibraryUserId == userId && x.BooksId == id)) + 2;

                    resultText = PlaceText(place);
                }
            }           
                            
            await _context.SaveChangesAsync();
            var result = new Dictionary<string, string> //to send two values
            {
                { "buttonText", buttonText },
                { "resultText", resultText }
            };
            return Json(result);
        }

        public IActionResult Favorites()
        {
            return View();
        }

        
        public async Task<IActionResult> GetFavoriteBooks([FromBody] string[] favoriteIdsString) //AJAX
        {
            if (favoriteIdsString == null || favoriteIdsString.Length == 0)
            {
                return BadRequest("No favorite book IDs provided.");
            }
            List<int> favoriteIds = favoriteIdsString.Select(int.Parse).ToList(); //make it int list
           
            List<Books> favoriteBooks = await _context.Books.Where(b => favoriteIds.Contains(b.Id)).ToListAsync();
            return PartialView("_ButtonListBooks", favoriteBooks); //instead of handling it in AJAX success, just send a partial view
            //return Ok(favoriteBooks);
        }
        private bool IsInWaitlist(int id)
        {
            string userId = _userManager.GetUserId(User); 
            if (_context.WaitList.Any(x => x.LibraryUserId == userId && x.BooksId == id))
            {
                return true;
            }
            
            return false;
        }

        private bool IsInHoldList(int id)
        {
            string userId = _userManager.GetUserId(User); 
            if (_context.Hold.Any(x => x.LibraryUserId == userId && x.BooksId == id))
            {
                return true;
            }

            return false;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangeComment(ChangeCommentViewModel model)
        {
            if (ModelState.IsValid)
            {
                Comments prevComment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == model.CommentId);
                prevComment.UpdateDate = DateTime.Now;
                prevComment.Comment = model.Comment;
                _context.Update(prevComment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = model.BooksId });
            }        
                             
            return Json(false);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment(CreateCommentViewModel model)
        {
            if (ModelState.IsValid)
            {
                Comments comment = new ()
                {
                    BooksId = model.BooksId,
                    Comment = model.Comment,
                    LibraryUserId = _userManager.GetUserId(User),
                    CreateDate = DateTime.Now,
                };
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = model.BooksId });
            }                     
            return Json(false);
        }

        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            string userId = _userManager.GetUserId(User); 
            if (userId != null)
            {
                Comments? comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.LibraryUserId == userId);
                if(comment != null)
                {
                    int bookId = comment.BooksId;
                    _context.Comments.Remove(comment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = bookId });
                }
                
            }
            return RedirectToAction("AccessDenied");
        }

        private string PlaceText(int place)
        {
            string placeText;
            if (place % 100 == 11 || place % 100 == 12 || place % 100 == 13) //exceptions
            {
                placeText = "th";
            }
            else
            {
                switch (place % 10) //ends with
                {
                    case 1:
                        placeText = "st";
                        break;
                    case 2:
                        placeText = "nd";
                        break;
                    case 3:
                        placeText = "rd";
                        break;
                    default:
                        placeText = "th";
                        break;
                }
            }
            string resultText = _localizer["You are in {0}{1} place in the wait list", place, placeText];
            return resultText;
        }
    }
}
