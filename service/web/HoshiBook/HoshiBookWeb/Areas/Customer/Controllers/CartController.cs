using HoshiBook.Utility;
using HoshiBook.Models;
using HoshiBookWeb.Tools;
using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models.ViewModels.Cart;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Stripe.Checkout;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;


namespace HoshiBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly string domain;
        private readonly ILogger<CartController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IDistributedCache _cache;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public int OrderTotal { get; set; }

        public CartController(
            IUnitOfWork unitOfWork, IEmailSender email, IConfiguration _config,
            ILogger<CartController> logger, IDistributedCache cache
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _emailSender = email;
            _cache = cache;
            domain = _config.GetValue<string>("DomainList:Kestrel:LocalDebug:Domain:https");
            // domain = _config.GetValue<string>("DomainList:Kestrel:LocalContainer:Domain:https");
            // domain = _config.GetValue<string>("DomainList:Kestrel:LocalContainer:Domain:http");
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == claim.Value,
                    includeProperties: "Product"),
                OrderHeader = new ()
            };
            foreach(var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(
                    cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100
                );
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                u => u.ApplicationUserId == claim.Value
            );
            if (cartFromDb == null)
            {
                return RedirectToAction(nameof(Index));
            }

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == claim.Value,
                    includeProperties: "Product"),
                    OrderHeader = new ()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(
                u => u.Id == claim.Value
            );

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            //TODO ApplicationUser StreetAddress, City, State, PostalCode maybe is null.
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress ?? "";
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City ?? "";
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State ?? "";
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode ?? "";

            foreach(var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(
                    cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100
                );
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST(ShoppingCartVM shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            try {
                ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == claim.Value,
                    includeProperties: "Product"
                );

                ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

                foreach(var cart in ShoppingCartVM.ListCart)
                {
                    cart.Price = GetPriceBasedOnQuantity(
                        cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100
                    );
                    ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
                }

                ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                if (applicationUser.CompanyId.GetValueOrDefault() == 0)
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                }
                else
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayed;
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                }

                _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
                _unitOfWork.Save();

                foreach(var cart in ShoppingCartVM.ListCart)
                {
                    OrderDetail orderDetail = new()
                    {
                        ProductId = cart.ProductId,
                        OrderId = ShoppingCartVM.OrderHeader.Id,
                        Price = cart.Price,
                        Count = cart.Count
                    };
                    _unitOfWork.OrderDetail.Add(orderDetail);
                    _unitOfWork.Save();
                }

                //TODO If current user is not company user, then create a stripe session.
                if (applicationUser.CompanyId.GetValueOrDefault() == 0)
                {
                    var options = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string>
                        {
                            "card"
                        },
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                        SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                        CancelUrl = domain + $"customer/cart/index",
                    };

                    foreach(var item in ShoppingCartVM.ListCart)
                    {
                        var sessionLineItem = new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(item.Price * 100), //20.00 -> 2000
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Product.Title
                                },
                            },
                            Quantity = item.Count,
                        };
                        options.LineItems.Add(sessionLineItem);
                    }

                    // _logger.LogInformation($"options.LineItems.Count: {options.LineItems.Count}");

                    var service = new SessionService();
                    Session session = service.Create(options);
                    
                    // _logger.LogInformation($"session id: {session.Id}");
                    // _logger.LogInformation($"session paymentIntentId: {session.PaymentIntentId}");

                    ShoppingCartVM.OrderHeader.SessionId = session.Id;
                    ShoppingCartVM.OrderHeader.PaymentIntentId = session.PaymentIntentId;

                    _unitOfWork.OrderHeader.UpdateStripePaymentID(
                        ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId
                    );
                    _unitOfWork.Save();

                    _logger.LogInformation($"session url: {session.Url}");

                    Response.Headers.Add("Location", session.Url);
                    return new StatusCodeResult(303);
                }
                else
                {
                    return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"SummaryPOST: {ExceptionTool.CollectDetailMessage(ex)}");
            }
            return View();
        }

        //TODO Maybe can be added 'lastUpdateTime' to OrderHeader table for record the payment data last update time
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
                u => u.Id == id,
                includeProperties: "ApplicationUser"
            );

            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayed)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check the stripe status
                if(session.PaymentStatus.ToLower() == "paid")
                {
                    _logger.LogInformation($"session id: {orderHeader.SessionId ?? "Unknown"}");
                    _logger.LogInformation($"session paymentIntentId: {session.PaymentIntentId}");

                    _unitOfWork.OrderHeader.UpdateStripePaymentID(
                        id, orderHeader.SessionId, session.PaymentIntentId
                    );
                    _unitOfWork.OrderHeader.UpdateStatus(
                        id, SD.StatusApproved, SD.PaymentStatusApproved
                    );
                    _unitOfWork.Save();
                }
            }
            _logger.LogInformation($"orderHeader.ApplicationUser.Email: {orderHeader.ApplicationUser.Email}");
            await _emailSender.SendEmailAsync(
                orderHeader.ApplicationUser.Email,
                "New Order - Hoshi Book",
                "<p>New Order Created</p>"
            );
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == orderHeader.ApplicationUserId
            );
            _cache.Remove(SD.SessionCart);
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }

        public IActionResult Plus(int? cartId)
        {
            if (cartId == null)
            {
                return NotFound();
            }
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == cart.ApplicationUserId
            );
            int count = shoppingCarts.Select(u => u.Count).Sum();
            _logger.LogInformation($"The user {cart.ApplicationUserId} has {count} items in the cart after increment product {cartId}.");
            _cache.SetString(SD.SessionCart, JsonSerializer.Serialize(count));
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int? cartId)
        {
            if (cartId == null)
            {
                return NotFound();
            }
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();
                List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == cart.ApplicationUserId
                );
                int count = shoppingCarts.Select(u => u.Count).Sum();
                _logger.LogInformation($"The user {cart.ApplicationUserId} has {count} items in the cart after clear product {cartId}.");
                _cache.SetString(SD.SessionCart, JsonSerializer.Serialize(count));
            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
                _unitOfWork.Save();
                List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == cart.ApplicationUserId
                );
                int count = shoppingCarts.Select(u => u.Count).Sum();
                _logger.LogInformation($"The user {cart.ApplicationUserId} has {count} items in the cart after decrement product {cartId}.");
                _cache.SetString(SD.SessionCart, JsonSerializer.Serialize(count));
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int? cartId)
        {
            if (cartId == null)
            {
                return NotFound();
            }
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == cart.ApplicationUserId
                );
            int count = shoppingCarts.Select(u => u.Count).Sum();
            _logger.LogInformation($"The user {cart.ApplicationUserId} has {count} items in the cart after remove.");
            _cache.SetString(SD.SessionCart, JsonSerializer.Serialize(count));
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else
            {
                if (quantity <= 100)
                {
                    return price50;
                }
                else
                {
                    return price100;
                }
            }
        }
    }
}