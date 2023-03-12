using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HoshiBook.Models;
using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Utility;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text;



namespace HoshiBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDistributedCache _cache;
        
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(
            ILogger<HomeController> logger, IUnitOfWork unitOfWork,
            IDistributedCache cache
        )
        {
            _logger = logger;
            _cache = cache;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return View(productList);
        }

        public IActionResult Details(int? productId)
        {
            if (productId == null)
            {
                return NotFound();
            }
            ShoppingCart cardObj = new ()
            {
                Count = 1,
                ProductId = productId.Value,
                Product = _unitOfWork.Product.GetFirstOrDefault(
                            u => u.Id == productId,
                            includeProperties: "Category,CoverType"
                        )
            };
            return View(cardObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            //TODO Get the product from the database.
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId
            );

            //TODO Check the product does exists in currently logged in user's cart.
            if (cartFromDb == null)
            {
                if (shoppingCart.Count < 1)
                {
                    TempData["error"] = "Add to car failed, because the product quantity is less than 1.";
                    return RedirectToAction(nameof(Details), new { productId = shoppingCart.ProductId });
                }
                else
                {
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                    _unitOfWork.Save();
                    //TODO Get the count of the single product in the cart.
                    int count = _unitOfWork.ShoppingCart.GetAll(
                        u => u.ApplicationUserId == claim.Value
                    ).Select(u => u.Count).Sum();
                    HttpContext.Session.SetInt32(SD.SessionCart, count);
                }
            }
            else
            {
                if (shoppingCart.Count < 1)
                {
                    TempData["error"] = "Add to car failed, because the product quantity is less than 1.";
                    return RedirectToAction(nameof(Details), new { productId = shoppingCart.ProductId });
                }
                else
                {
                    TempData["success"] = $"Add to car sucessfully.";
                    _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
                    _unitOfWork.Save();
                    //TODO Get the count of the single product in the cart.
                    int count = _unitOfWork.ShoppingCart.GetAll(
                        u => u.ApplicationUserId == claim.Value
                    ).Select(u => u.Count).Sum();
                    HttpContext.Session.SetInt32(SD.SessionCart, count);
                }
            }
            return RedirectToAction(nameof(Index));
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
