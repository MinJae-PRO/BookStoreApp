using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreApp.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column("total_price")]
        public decimal TotalPrice { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; } = default!;

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
