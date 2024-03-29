using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HoshiBook.Models.ViewModels.User
{
    public class UserDetailsVM
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string StreetAddress { get; set; } = default!;
        public string City { get; set; } = default!;
        public string State { get; set; } = default!;
        public string PostalCode { get; set; } = default!;
        public string RoleName { get; set; } = default!;
        public string CompanyName { get; set; } = default!;
        public string IsLockedOut { get; set; } = default!;
        public string CreatedAt { get; set; } = default!;
        public string ModifiedAt { get; set; } = default!;
        public string LastLoginTime { get; set; } = default!;
        public string LoginIPv4Address { get; set; } = default!;
        public string LastTryLoginTime { get; set; } = default!;
        public int AccessFailedCount { get; set; } = default!;
        public int RoleNumber { get; set; } = default!;
    }
}