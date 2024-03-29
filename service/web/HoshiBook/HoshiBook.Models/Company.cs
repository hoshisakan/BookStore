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
    [Table("Companies", Schema = "bookstore")]
    public class Company
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = default!;
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        [ValidateNever]
        public ICollection<ApplicationUser> ApplicationUsers { get; set; }
    }
}