using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoshiBook.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public List<ShoppingCart> ListCart { get; set; } = default!;
    }
}