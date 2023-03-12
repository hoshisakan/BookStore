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
                int? temp = 0;
                // if (_cache.GetString(SD.SessionCart) != null)
                // {
                //     return View(_cache.GetString(SD.SessionCart));
                // }
                // else
                // {
                //     int count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Select(u => u.Count).Sum();
                //     // _cache.SetString(SD.SessionCart, count.ToString());
                //     _cache.SetString(SD.SessionCart, JsonSerializer.Serialize(count));
                //     return View(_cache.GetString(SD.SessionCart));
                // }
                if (HttpContext.Session.GetInt32(SD.SessionCart) != null)
                {
                    temp = HttpContext.Session.GetInt32(SD.SessionCart);
                    _logger.LogInformation("ShoppingCartViewComponent.InvokeAsync: tempA = " + temp.ToString());
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }
                else
                {
                    HttpContext.Session.SetInt32(
                        SD.SessionCart,
                        _unitOfWork.ShoppingCart.GetAll(
                            u => u.ApplicationUserId == claim.Value
                        ).Select(u => u.Count).Sum()
                    );
                    temp = HttpContext.Session.GetInt32(SD.SessionCart);
                    _logger.LogInformation("ShoppingCartViewComponent.InvokeAsync: tempB = " + temp.ToString());
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                    // return View(1);
                }
            }
            else
            {
                _logger.LogInformation("ShoppingCartViewComponent.InvokeAsync: claim is null");
                HttpContext.Session.Clear();
                // _cache.Remove(SD.SessionCart);
                return View(0);
            }
        }
    }
}