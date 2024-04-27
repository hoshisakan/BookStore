using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HoshiBook.Models.ViewModels.User
{
    public class UserEditVM
    {
        public ApplicationUser ApplicationUser { get; set; } = default!;
        public ApplicationRole ApplicationRole { get; set; } = default!;
        [ValidateNever]
        public IEnumerable<SelectListItem> CompanyList { get; set; } = default!;
        [ValidateNever]
        public IEnumerable<SelectListItem> RoleList { get; set; } = default!;
        public string? CompanyId { get; set; }
        public string? RoleId { get; set; }
    }
}