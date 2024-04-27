using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HoshiBook.Models
{
    public class ApplicationUser : IdentityUser
    {        
        [Required]
        public string Name { get; set; } = default!;
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public int? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        [ValidateNever]
        public Company Company { get; set; } = default!;
        public bool IsLockedOut { get; set; }
        public string? LoginIPv4Address { get; set; }
        public string? LoginIPv6Address { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastTryLoginTime { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
    }
}