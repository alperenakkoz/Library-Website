using Library.Models;
using Library.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Library.Controllers
{

    public class AccountController : Controller
    {
        private readonly UserManager<LibraryUser> _userManager;
        private readonly SignInManager<LibraryUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly ISenderEmail _emailSender;
        private readonly IStringLocalizer<AccountController> _localizer;

        public AccountController(UserManager<LibraryUser> userManager, SignInManager<LibraryUser> signInManager, 
                                 AppDbContext context, ISenderEmail emailSender, IStringLocalizer<AccountController> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailSender = emailSender;
            _localizer = localizer;
        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                LibraryUser user = new () //UserName = Email
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SendConfirmationEmail(model.Email, user, null);
                    await _userManager.AddToRoleAsync(user, "Member"); //default role
                    return View("RegistrationSuccessful");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? ReturnUrl = null) //ReturnUrl holds the link of the page that directed to the login page.
        {
            ReturnUrl ??= Request.Headers.Referer; //If user clicked the login, get previous page
            ViewData["ReturnUrl"] = ReturnUrl; 
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.EmailConfirmed && (await _userManager.CheckPasswordAsync(user, model.Password)))
                {
                    ModelState.AddModelError(string.Empty, _localizer["EmailNotConfirmed"]);
                    return View(model);
                }
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                        return RedirectToAction("Index", "Books"); //return main page
                }

                else
                {
                    ModelState.AddModelError(string.Empty, _localizer["InvalidLogin"]);
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();    
            if(!String.IsNullOrEmpty(Request.Headers.Referer))
                return Redirect(Request.Headers.Referer); //returns the page which you log out from
            return RedirectToAction("Index", "Books");
        }

        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> IsEmailAvailable(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                return Json(true);
            }
            else
            {
                var loggedInUser = await _userManager.GetUserAsync(User); 
                if (loggedInUser != null && loggedInUser.Email == Email) //same function used for user e-mail update. So, we need to check
                {
                    return Json(true);
                }
                return Json($"{Email} " + _localizer["AlreadyInUse"]);
            }
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            LibraryUser user = await _userManager.GetUserAsync(User);
            List<Books> reservedBooks = await _context.Books.Where(x => _context.Hold
                                                .Where(h => h.LibraryUserId == user.Id)
                                                .Select(h => h.BooksId)
                                                .Contains(x.Id)).ToListAsync();
            var today = DateTime.Today;
            List<Books> rentedBooks = await _context.Books.Where(x => _context.Rent
                                                .Where(h => h.LibraryUserId == user.Id && !h.IsReturned && h.EndTime.Date >= today.Date)
                                                .Select(h => h.BooksId)
                                                .Contains(x.Id)).ToListAsync();

            List<Books> OverdueBooks = await _context.Books.Where(x => _context.Rent
                                                .Where(h => h.LibraryUserId == user.Id && !h.IsReturned  && h.EndTime.Date < today.Date)
                                                .Select(h => h.BooksId)
                                                .Contains(x.Id)).ToListAsync();
            List<Books> waitListBooks = await _context.Books.Where(x => _context.WaitList
                                               .Where(h => h.LibraryUserId == user.Id)
                                               .Select(h => h.BooksId)
                                               .Contains(x.Id)).ToListAsync();
            List<Books> prevRentedBooks = await _context.Books.Where(x => _context.Rent
                                                .Where(h => h.LibraryUserId == user.Id && h.IsReturned)
                                                .Select(h => h.BooksId)
                                                .Contains(x.Id)).ToListAsync();
            UserViewModel userViewModel = new ()
            {
                UserId = user.Id,
                ReservedBooks = reservedBooks,
                RentedBooks = rentedBooks,
                OverdueBooks = OverdueBooks,
                WaitListBooks = waitListBooks,
                PrevRentedBooks = prevRentedBooks
            };
            return View(userViewModel);
        }

        private async Task SendConfirmationEmail(string? email, LibraryUser? user, string? newMail)
        {
            string? token;
            string? ConfirmationLink;
            if (newMail == null) //used for confirm the email after registration
            {
                token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                ConfirmationLink = Url.Action("ConfirmEmail", "Account", new { UserId = user.Id, Token = token }, protocol: HttpContext.Request.Scheme);
            }
            else //used for confirm the email after email change
            {
                token = await _userManager.GenerateChangeEmailTokenAsync(user, newMail);
                ConfirmationLink = Url.Action("ConfirmEmail", "Account", new { UserId = user.Id, Token = token, Email = newMail }, protocol: HttpContext.Request.Scheme);
            }         

            await _emailSender.SendEmailAsync(email, _localizer["ConfirmYourEmail"], _localizer["ConfirmEmailText"] + $" <a href='{HtmlEncoder.Default.Encode(ConfirmationLink)}'>" + _localizer["ClickHere"] +"</a>.", true);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string UserId, string Token, string? Email)
        {
            ViewData["Title"] = _localizer["ConfirmEmailTitle"];
            if (UserId == null || Token == null)
            {
                ViewBag.Message = _localizer["InvalidOrExpired"];
            }

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = _localizer["UserIDInvalid"];
                return View("NotFound");
            }

            if(Email == null) //registration
            {
                var result = await _userManager.ConfirmEmailAsync(user, Token);
                if (result.Succeeded)
                {
                    ViewBag.Message = _localizer["ConfirmMessage"];
                    return View();
                }
            }
            else //change email
            {
                var result = await _userManager.ChangeEmailAsync(user, Email, Token);
                await _userManager.SetUserNameAsync(user, Email);
                if (result.Succeeded)
                {
                    ViewBag.Message = _localizer["ConfirmMessage"];
                    await _signInManager.RefreshSignInAsync(user);
                    return View();
                }
            }
            
            return View();
        }

        [AllowAnonymous]
        public IActionResult ResendConfirmationEmail(bool IsResend = true)
        {
            ViewBag.placeholder = _localizer["MailPlaceHolder"];
            if (IsResend)
            {
                ViewBag.Message = _localizer["ResendConfirmationEmail"];
            }
            else
            {
                ViewBag.Message = _localizer["SendConfirmationEmail"];
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmationEmail(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null || await _userManager.IsEmailConfirmedAsync(user))
            {
                return View("ConfirmationEmailSent");
            }

            await SendConfirmationEmail(Email, user, null);

            return View("ConfirmationEmailSent");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
               
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    await SendForgotPasswordEmail(user.Email, user);
                    return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            return View(model);
        }

        private async Task SendForgotPasswordEmail(string? email, LibraryUser? user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var passwordResetLink = Url.Action("ResetPassword", "Account",
                    new { Email = email, Token = token }, protocol: HttpContext.Request.Scheme);
         
            await _emailSender.SendEmailAsync(email, _localizer["ResetYourPassword"], _localizer["PasswordResetText"] + $" <a href='{HtmlEncoder.Default.Encode(passwordResetLink)}'>" + _localizer["ClickHere"] +"</a>.", true);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string Token, string Email)
        {
            if (Token == null || Email == null)
            {
                ViewBag.ErrorTitle = _localizer["InvalidToken"];
                ViewBag.ErrorMessage = _localizer["InvalidOrExpired"];
                return View("Error");
            }
            else
            {
                ResetPasswordViewModel model = new()
                {
                    Token = Token,
                    Email = Email
                };
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ResetPasswordConfirmation", "Account");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                await _signInManager.RefreshSignInAsync(user);

                TempData["ChangeMessage"] = (string) _localizer["PasswordChangeMessage"];
                return RedirectToAction("Settings", "Account");
            }

            return View(model);
        }

      

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Settings() //user credentials page
        {
            var user = await _userManager.GetUserAsync(User);
            UserInfoViewModel userInfoViewModel = new ()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
            return View(userInfoViewModel);
        }

        [Authorize]
        public async Task<IActionResult> ChangeSettings()//change user credentials page
        {
            var user = await _userManager.GetUserAsync(User);
            UserInfoViewModel userInfoViewModel = new ()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
            return View(userInfoViewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangeSettings(UserInfoViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    if (user.Email == model.Email)
                    {
                        user.FirstName = model.FirstName;
                        user.LastName = model.LastName;
                        await _userManager.UpdateAsync(user);
                        TempData["ChangeMessage"] = (string) _localizer["InfoChangeMessage"];
                    }
                    else
                    {
                        user.FirstName = model.FirstName;
                        user.LastName = model.LastName;
                        await _userManager.UpdateAsync(user);
                        await SendConfirmationEmail(model.Email, user, model.Email);
                        TempData["ChangeMessage"] = (string) _localizer["MailChangeMessage"];
                    }
                }               
                return RedirectToAction("Settings");
            }          
            return View(model);
        }

        public IActionResult ChangeLanguage(string culture) //select the language
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTime.Now.AddYears(1) }
            );
            string? returnUrl = Request.Headers.Referer;
            if(returnUrl != null)
                return Redirect(returnUrl);         
            return RedirectToAction("Index", "Books");
        }
    }
}
