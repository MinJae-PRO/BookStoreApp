using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreApp.Models
{
    [Table("BookReviews")]
    public class BookReview
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("book_id")]
        public int BookId { get; set; }

        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        [Column("rating")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required.")]
        [StringLength(1000)]
        [Column("comment")]
        public string Comment { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("BookId")]
        public Book Book { get; set; } = default!;

        [ForeignKey("UserId")]
        public AppUser User { get; set; } = default!;
    }
}
