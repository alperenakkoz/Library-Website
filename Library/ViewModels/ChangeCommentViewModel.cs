using Library.Models;
using System.ComponentModel.DataAnnotations;

namespace Library.ViewModels
{
    public class ChangeCommentViewModel
    {
        public int CommentId { get; set; }
        public int BooksId { get; set; }

        [Required(ErrorMessage = "CommentRequired")]
        public string Comment { get; set; }      
    }
}
