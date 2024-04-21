using Library.Models;

namespace Library.ViewModels
{
    public class UserViewModel
    {
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public List<Books>? OverdueBooks { get; set; }
        public List<Books>? ReservedBooks { get; set; }
        public List<Books>? RentedBooks { get; set; }
        public PaginatedList<Books>? PaginatedBooks { get; set; }
        public List<Books>? WaitListBooks { get; set; }
        public List<Books>? PrevRentedBooks { get; set; }
    }
}
