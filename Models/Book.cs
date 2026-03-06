using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreApp.Models
{
    [Table("Books")]
    public class Book
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author name is required.")]
        [Column("author")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required.")]
        [StringLength(60)]
        [Column("genre")]
        public string Genre { get; set; } = string.Empty;

        [Required]
        [Column("price")]
        public decimal Price { get; set; }
    }
}