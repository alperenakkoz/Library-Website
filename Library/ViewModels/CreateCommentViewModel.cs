using Library.Models;
using System.ComponentModel.DataAnnotations;

namespace Library.ViewModels
{
    public class CreateCommentViewModel
    {
        [Required(ErrorMessage = "CommentRequired")]
        public string Comment { get; set; }
        public int BooksId { get; set; }             
    }
}
