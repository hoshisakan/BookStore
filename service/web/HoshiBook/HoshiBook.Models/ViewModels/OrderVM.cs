using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HoshiBook.Models.ViewModels
{
    public class OrderVM
    {
        public OrderHeader OrderHeader { get; set; } = default!;
        public List<OrderDetail> OrderDetail { get; set; } = default!;
    }
}