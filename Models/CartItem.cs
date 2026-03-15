using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreApp.Models
{
    [Table("CartItems")]
    public class CartItem
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

        [Required]
        [Range(1, int.MaxValue)]
        [Column("quantity")]
        public int Quantity { get; set; } = 1;

        [ForeignKey("UserId")]
        public AppUser User { get; set; } = default!;

        [ForeignKey("BookId")]
        public Book Book { get; set; } = default!;
    }
}
