using Library.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Library
{

    public class ReturnDatePassWarning
    {
        private readonly AppDbContext _context;
        private readonly UserManager<LibraryUser> _userManager;
        public ReturnDatePassWarning(AppDbContext context, UserManager<LibraryUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public bool HasOverdueBooks(string userName)
        {
            if (userName.IsNullOrEmpty())
            {
                return false;
            }
            var today = DateTime.Today;
            string userId = _userManager.Users.Where(x=> x.UserName == userName).Select(x=> x.Id).First();
            bool res = _context.Rent.Any(h =>
                h.LibraryUserId == userId &&
                h.IsReturned == false &&
                h.EndTime.Date < today.Date
            );
            return res;
        }
    }
}
