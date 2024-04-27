using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HoshiBook.Models
{
    [Table("ShoppingCarts", Schema = "bookstore")]
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; } = default!;
        // [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
        [Required]
        [Range(1, 1000)]
        public int Count { get; set; }
        public string ApplicationUserId { get; set; } = default!;
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }= default!;
        [NotMapped]
        public double Price { get; set; }
    }
}