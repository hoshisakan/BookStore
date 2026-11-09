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
            // 從資料庫取得所有商品，並包含分類與封面資料
            List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return View(productList); // 傳給前端 View 顯示商品
        }

        public IActionResult Details(string? productId)
        {
            if (productId == null)
            {
                return NotFound();
            }

            // 根據 SKU 取得商品（包含分類與封面類型）
            var product = _unitOfWork.Product.GetFirstOrDefault(
                u => u.SKU == productId,
                includeProperties: "Category,CoverType"
            );
            if (product == null)
            {
                return NotFound(); // 找不到商品
            }

            // 準備加入購物車的資料（初始數量為 1）
            ShoppingCart cartObj = new ()
            {
                Count = 1,
                ProductId = product.Id,
                Product = _unitOfWork.Product.GetFirstOrDefault(
                            u => u.SKU == productId,
                            includeProperties: "Category,CoverType"
                        )
            };

            // 建立 ViewModel 包含 SKU 與購物車資料
            ShoppingCartForSKUVM cartAndSKUObj = new ()
            {
                SKU = productId,
                ShoppingCart = cartObj
            };
            return View(cartAndSKUObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // 必須登入才能加入購物車
        public IActionResult Details(ShoppingCartForSKUVM obj)
        {
            ShoppingCart shoppingCart = new();
            // int _productRealId = 0;
            try
            {
                shoppingCart = obj.ShoppingCart;

                // 取得目前登入使用者 ID
                var claimsIdentity = (ClaimsIdentity?)User.Identity;
                var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCart.ApplicationUserId = claim.Value;


                // 查詢該使用者是否已將此商品加入購物車
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


                // 若購物車中沒有該商品
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
                        // 新增至購物車資料表
                        _unitOfWork.ShoppingCart.Add(shoppingCart);
                        _unitOfWork.Save();

                        // 更新快取中的購物車數量
                        // TODO Get the count of the single product in the cart.
                        List<ShoppingCart> cartList = _unitOfWork.ShoppingCart.GetAll(
                            u => u.ApplicationUserId == claim.Value
                        );
                        int count = cartList.Select(u => u.Count).Sum();
                        _cache.SetString(SD.SessionCart, JsonSerializer.Serialize(count));
                    }
                }
                else // 若已存在此商品於購物車
                {
                    if (shoppingCart.Count < 1)
                    {
                        TempData["error"] = "Add to car failed, because the product quantity is less than 1.";
                        return RedirectToAction(nameof(Details), new { productId = obj.SKU });
                    }
                    else
                    {
                        // 增加現有商品數量
                        TempData["success"] = $"Add to car sucessfully.";
                        _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
                        _unitOfWork.Save();

                        // 更新快取中的購物車總數量
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
                // 發生例外時回報錯誤並回到商品頁
                TempData["error"] = $"Add to car failed, because {ex.Message}";
                _logger.LogError(ex, $"Add to car failed, because {ex.Message}\n{ex.StackTrace}");
                return RedirectToAction(nameof(Details), new { productId = obj.SKU });
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View(); // 隱私權政策頁
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // 錯誤頁面顯示 RequestId（用於錯誤追蹤）
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
