using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HoshiBook.Models
{
    [Table("Products", Schema = "bookstore")]
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        [Required]
        public string ISBN { get; set; } = default!;
        [Required]
        public string Author { get; set; } = default!;
        [Required]
        [Range(1, 10000)]
        [Display(Name = "List Price")]
        public double ListPrice { get; set; }
        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 1-50")]
        public double Price { get; set; }
        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 50-100")]
        public double Price50 { get; set; }
        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 100+")]
        public double Price100 { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; } = default!;
        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; } = default!;
        [Required]
        [Display(Name = "Cover Type")]
        public int CoverTypeId { get; set; }
        [ForeignKey("CoverTypeId")]
        [ValidateNever]
        public CoverType CoverType { get; set; } = default!;
    }
}