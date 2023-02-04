using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models.ViewModels;
using System.Security.Claims;
using HoshiBook.Utility;
using HoshiBook.Models;
using HoshiBookWeb.Tools;
using Stripe.Checkout;

namespace HoshiBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public int OrderTotal { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart =_unitOfWork.ShoppingCart.GetAll(
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

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart =_unitOfWork.ShoppingCart.GetAll(
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

                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

                foreach(var cart in ShoppingCartVM.ListCart)
                {
                    cart.Price = GetPriceBasedOnQuantity(
                        cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100
                    );
                    ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
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

                //stripe settings
                var domain = "https://localhost:7229/";
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

                Console.WriteLine($"options.LineItems.Count: {options.LineItems.Count}");

                var service = new SessionService();
                Session session = service.Create(options);
                
                Console.WriteLine($"session id: {session.Id}");
                Console.WriteLine($"session paymentIntentId: {session.PaymentIntentId}");

                ShoppingCartVM.OrderHeader.SessionId = session.Id;
                ShoppingCartVM.OrderHeader.PaymentIntentId = session.PaymentIntentId;

                _unitOfWork.OrderHeader.UpdateStripePaymentID(
                    ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId
                );
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            catch (Exception ex)
            {
                ExceptionTool.CollectDetailMessage(ex);
            }
            return View();
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            //check the stripe status
            if(session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(
                    id, SD.StatusApproved, SD.PaymentStatusApproved
                );
                _unitOfWork.Save();
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == orderHeader.ApplicationUserId
            ).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }

        public IActionResult Plus(int? cardId)
        {
            if (cardId == null)
            {
                return NotFound();
            }
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cardId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int? cardId)
        {
            if (cardId == null)
            {
                return NotFound();
            }
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cardId);
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int? cardId)
        {
            if (cardId == null)
            {
                return NotFound();
            }
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cardId);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
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