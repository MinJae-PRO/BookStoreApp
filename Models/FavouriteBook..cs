using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreApp.Models
{
    [Table("FavoriteBooks")]
    public class FavouriteBook
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column("book_id")]
        public int BookId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; } = default!;

        [ForeignKey("BookId")]
        public Book Book { get; set; } = default!;
    }
}
