using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using HoshiBook.Models;
using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models.ViewModels;

namespace HoshiBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return View(productList);
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ShoppingCart cardObj = new ()
            {
                Count = 1,
                Product = _unitOfWork.Product.GetFirstOrDefault(
                            u => u.Id == id,
                            includeProperties: "Category,CoverType"
                        )
            };
            return View(cardObj);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
