using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HoshiBook.Models;
using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Utility;
using HoshiBook.Models.ViewModels.Cart;


using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

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

        public ActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return View(productList);
        }

        public IActionResult Details(string? productId)
        {
            if (productId == null)
            {
                return NotFound();
            }
            var product = _unitOfWork.Product.GetFirstOrDefault(
                u => u.SKU == productId,
                includeProperties: "Category,CoverType"
            );
            if (product == null)
            {
                return NotFound();
            }
            ShoppingCart cartObj = new ()
            {
                Count = 1,
                ProductId = product.Id,
                Product = _unitOfWork.Product.GetFirstOrDefault(
                            u => u.SKU == productId,
                            includeProperties: "Category,CoverType"
                        )
            };
            ShoppingCartForSKUVM cartAndSKUObj = new ()
            {
                SKU = productId,
                ShoppingCart = cartObj
            };
            return View(cartAndSKUObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCartForSKUVM obj)
        {
            ShoppingCart shoppingCart = new();
            // int _productRealId = 0;
            try
            {
                shoppingCart = obj.ShoppingCart;

                var claimsIdentity = (ClaimsIdentity?)User.Identity;
                var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCart.ApplicationUserId = claim.Value;

                //TODO Get the product from the database.
                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                    u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId
                );

                _logger.LogInformation($"shoppingCart.Count: {shoppingCart.Count}");
                _logger.LogInformation($"_productRealId: {shoppingCart.ProductId}");
                _logger.LogInformation(
                    $"shoppingCart Id: {0}, ProductId: {1}, Count: {2}, ApplicationUserId: {3}",
                    shoppingCart.Id, shoppingCart.ProductId, shoppingCart.Count, shoppingCart.ApplicationUserId
                );

                //TODO Check the product does exists in currently logged in user's cart.
                if (cartFromDb == null)
                {
                    if (shoppingCart.Count < 1)
                    {
                        TempData["error"] = "Add to car failed, because the product quantity is less than 1.";
                        return RedirectToAction(nameof(Details), new { productId = obj.SKU });
                    }
                    else
                    {
                        _unitOfWork.ShoppingCart.Add(shoppingCart);
                        _unitOfWork.Save();
                        // TODO Get the count of the single product in the cart.
                        List<ShoppingCart> cartList = _unitOfWork.ShoppingCart.GetAll(
                            u => u.ApplicationUserId == claim.Value
                        );
                        int count = cartList.Select(u => u.Count).Sum();
                        _cache.SetString(SD.SessionCart, JsonSerializer.Serialize(count));
                    }
                }
                else
                {
                    if (shoppingCart.Count < 1)
                    {
                        TempData["error"] = "Add to car failed, because the product quantity is less than 1.";
                        return RedirectToAction(nameof(Details), new { productId = obj.SKU });
                    }
                    else
                    {
                        TempData["success"] = $"Add to car sucessfully.";
                        _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
                        _unitOfWork.Save();
                        // TODO Get the count of the single product in the cart.
                        List<ShoppingCart> cartList = _unitOfWork.ShoppingCart.GetAll(
                            u => u.ApplicationUserId == claim.Value
                        );
                        int count = cartList.Select(u => u.Count).Sum();
                        _cache.SetString(SD.SessionCart, JsonSerializer.Serialize(count));
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Add to car failed, because {ex.Message}";
                _logger.LogError(ex, $"Add to car failed, because {ex.Message}\n{ex.StackTrace}");
                return RedirectToAction(nameof(Details), new { productId = obj.SKU });
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
