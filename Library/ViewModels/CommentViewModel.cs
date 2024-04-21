using Library.Models;

namespace Library.ViewModels
{
    public class CommentViewModel
    {
        public string Comment { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime? UpdateDate { get; set; }
    }
}
