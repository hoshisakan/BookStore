using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models;
using HoshiBook.Utility;
using HoshiBook.Models.ViewModels;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace HoshiBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly string domain;
        private readonly ILogger<OrderController> _logger;

        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM? OrderVM { get; set; }

        public OrderController(ILogger<OrderController> logger, IUnitOfWork unitOfWork, IConfiguration _config)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            // domain = _config.GetValue<string>("DomainList:Kestrel:LocalDebug:Domain:https");
            domain = _config.GetValue<string>("DomainList:Kestrel:LocalContainer:Domain:https");
            // domain = _config.GetValue<string>("DomainList:Kestrel:LocalContainer:Domain:http");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
                    u => u.Id == orderId, includeProperties: "ApplicationUser"
                ),
                OrderDetail =_unitOfWork.OrderDetail.GetAll(
                    u => u.OrderId == orderId, includeProperties: "Product"
                )
            };
            return View(OrderVM);
        }

        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_PAY_NOW(int orderId)
        {
            OrderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
                u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser"
            );
            OrderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(
                    u => u.OrderId == OrderVM.OrderHeader.Id, includeProperties: "Product"
            );
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderID={OrderVM.OrderHeader.Id}",
            };

            foreach(var item in OrderVM.OrderDetail)
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

            _logger.LogInformation($"options.LineItems.Count: {options.LineItems.Count}");

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeader.UpdateStripePaymentID(
                OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId
            );
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderid)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderid);

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayed)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check the stripe status
                if(session.PaymentStatus.ToLower() == "paid")
                {
                    _logger.LogInformation($"session id: {orderHeader.SessionId ?? "Unknown"}");
                    _logger.LogInformation($"session paymentIntentId: {session.PaymentIntentId}");

                    _unitOfWork.OrderHeader.UpdateStripePaymentID(
                        orderHeaderid, orderHeader.SessionId ?? "", session.PaymentIntentId
                    );
                    _unitOfWork.OrderHeader.UpdateStatus(
                        orderHeaderid, orderHeader.OrderStatus ?? "", SD.PaymentStatusApproved
                    );
                    _unitOfWork.Save();
                }
            }
            return View(orderHeaderid);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail()
        {
            if (OrderVM == null)
            {
                throw new ArgumentNullException($"OrderVM is null.");
            }
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(
                u => u.Id == OrderVM.OrderHeader.Id, tracked: false
            );
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

            if (OrderVM.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            orderHeaderFromDb.LastUpdateTime = DateTime.Now;
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            if (OrderVM == null)
            {
                throw new ArgumentNullException($"OrderVM is null.");
            }
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
                u => u.Id == OrderVM.OrderHeader.Id, tracked: false
            );
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProgress, SD.StatusInProgress);
            _unitOfWork.Save();
            TempData["Success"] = "Order Status Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            if (OrderVM == null)
            {
                throw new ArgumentNullException($"OrderVM is null.");
            }
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
                u => u.Id == OrderVM.OrderHeader.Id, tracked: false
            );
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            // orderHeader.PaymentStatus = SD.PaymentStatusDelayed;
            orderHeader.PaymentStatus = SD.StatusShipped;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayed)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            orderHeader.LastUpdateTime = DateTime.Now;
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            if (OrderVM == null)
            {
                throw new ArgumentNullException($"OrderVM is null.");
            }
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
                u => u.Id == OrderVM.OrderHeader.Id, tracked: false
            );

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(
                    orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded
                );
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(
                    orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled
                );
            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            List<OrderHeader> orderHeaders;

            orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity?)User.Identity;
                var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
                // _logger.LogInformation($"Claim: {claim?.Value}");
                orderHeaders = _unitOfWork.OrderHeader.GetAll(
                    u => u.ApplicationUserId == claim.Value,
                    includeProperties: "ApplicationUser"
                );
            }

            switch (status)
            {
                case "pending":
                    _logger.LogInformation($"Pending");
                    orderHeaders = orderHeaders.Where(
                        u => u.PaymentStatus == SD.PaymentStatusDelayed
                    ).ToList();
                    break;
                case "inprocess":
                    _logger.LogInformation($"In Process");
                    orderHeaders = orderHeaders.Where(
                        u => u.PaymentStatus == SD.StatusInProgress
                    ).ToList();
                    break;
                case "completed":
                    _logger.LogInformation($"Completed");
                    orderHeaders = orderHeaders.Where(
                        u => u.PaymentStatus == SD.StatusShipped
                    ).ToList();
                    break;
                case "approved":
                    _logger.LogInformation($"Approved");
                    orderHeaders = orderHeaders.Where(
                        u => u.PaymentStatus == SD.StatusApproved
                    ).ToList();
                    break;
                default:
                    _logger.LogInformation($"Other Status: {status}");
                    break;
            }
            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}