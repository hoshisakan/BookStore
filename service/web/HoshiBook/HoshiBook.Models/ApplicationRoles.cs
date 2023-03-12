using Microsoft.AspNetCore.Identity;

namespace HoshiBook.Models
{
    public class ApplicationRole : IdentityRole
    {
        public int RoleNumber { get; set; } = default!;
    }
}