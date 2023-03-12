using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Utility;

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text;

namespace HoshiBookWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork, IDistributedCache cache, ILogger<ShoppingCartViewComponent> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                _logger.LogInformation("ShoppingCartViewComponent.InvokeAsync: claim isn't null");
                int? tempCacheCartCount = 0;
                if (_cache.GetString(SD.SessionCart) != null)
                {
                    tempCacheCartCount = JsonSerializer.Deserialize<int>(_cache.GetString(SD.SessionCart));
                    _logger.LogInformation($"tempCacheCartCount: {tempCacheCartCount}");
                    return View(tempCacheCartCount);
                }
                else
                {
                    tempCacheCartCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Select(u => u.Count).Sum();
                    _logger.LogInformation($"tempCacheCartCount: {tempCacheCartCount}");
                    _cache.SetString(SD.SessionCart, JsonSerializer.Serialize(tempCacheCartCount));
                    return View(tempCacheCartCount);
                }
            }
            else
            {
                _logger.LogInformation("ShoppingCartViewComponent.InvokeAsync: claim is null");
                _cache.Remove(SD.SessionCart);
                return View(0);
            }
        }
    }
}