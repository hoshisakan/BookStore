using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HoshiBook.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; } = default!;
        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryList { get; set; } = default!;
        [ValidateNever]
        public IEnumerable<SelectListItem> CoverTypeList { get; set; } = default!;
    
    }
}