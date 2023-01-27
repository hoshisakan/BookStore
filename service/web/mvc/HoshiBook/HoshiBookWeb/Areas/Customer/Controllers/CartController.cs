using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HoshiBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}