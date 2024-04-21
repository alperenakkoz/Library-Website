using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class Notification
    {
        [Column("NotificationId")]
        public int Id { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public string Message { get; set; }
        public string LibraryUserId { get; set; }
        public LibraryUser LibraryUser { get; set; }
    }
}
