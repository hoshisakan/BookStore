using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Utility;

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text;
using HoshiBook.Models;

namespace HoshiBookWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        // 建構子注入必要的服務：資料存取、分散式快取與 logger
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork, IDistributedCache cache, ILogger<ShoppingCartViewComponent> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        // 此方法會自動於 Razor 中 @await Component.InvokeAsync("ShoppingCart") 被呼叫
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // 從目前登入的使用者取得使用者識別 Claim
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // 確認使用者已登入
            if (claim != null)
            {
                _logger.LogInformation("ShoppingCartViewComponent.InvokeAsync: claim isn't null");
                int? tempCacheCartCount = 0;

                // 從 Redis 或其他實作的快取中取得購物車數量（避免頻繁查 DB）
                if (_cache.GetString(SD.SessionCart) != null)
                {
                    // 從快取反序列化購物車商品總數
                    tempCacheCartCount = JsonSerializer.Deserialize<int>(_cache.GetString(SD.SessionCart));
                    _logger.LogInformation($"tempCacheCartCount: {tempCacheCartCount}");

                    // 回傳購物車商品數給 ViewComponent 的 View
                    return View(tempCacheCartCount);
                }
                else
                {
                    // 若快取為空，則從資料庫查詢目前使用者的所有購物車項目
                    List<ShoppingCart> shoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                        u => u.ApplicationUserId == claim.Value
                    );

                    // 計算商品總數（例如：A*2 + B*1 = 3）
                    tempCacheCartCount = shoppingCartList.Select(u => u.Count).Sum();
                    _logger.LogInformation($"tempCacheCartCount: {tempCacheCartCount}");

                    // 寫入快取（key 為 SD.SessionCart，value 為總數量的 JSON 字串）
                    _cache.SetString(SD.SessionCart, JsonSerializer.Serialize(tempCacheCartCount));
                    return View(tempCacheCartCount);
                }
            }
            else
            {
                // 若使用者未登入，則清除快取並回傳 0
                _logger.LogInformation("ShoppingCartViewComponent.InvokeAsync: claim is null");
                _cache.Remove(SD.SessionCart); // 移除任何舊的快取
                return View(0); // 未登入預設顯示為 0
            }
        }
    }
}