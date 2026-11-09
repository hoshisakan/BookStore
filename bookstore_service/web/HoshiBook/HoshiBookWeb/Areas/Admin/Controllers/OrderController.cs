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
            domain = _config.GetValue<string>("DomainList:Kestrel:LocalDebug:Domain:https");
            // 若要使用容器化部署時的域名，請取消以下註解
            //domain = _config.GetValue<string>("DomainList:Kestrel:LocalContainer:Domain:https");
            // domain = _config.GetValue<string>("DomainList:Kestrel:LocalContainer:Domain:http");
        }

        public IActionResult Index()
        {
            return View();
        }


        // 顯示訂單明細頁面（GET）
        public IActionResult Details(int orderId)
        {
            // 建立一個新的 OrderVM，並透過傳入的 orderId 撈取訂單主檔與明細資料
            OrderVM = new OrderVM()
            {
                // 取得訂單主檔資料，包含關聯的 ApplicationUser 資料
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
                    u => u.Id == orderId, includeProperties: "ApplicationUser"
                ),
                // 取得所有該訂單編號的訂單明細，包含關聯的 Product 資料
                OrderDetail = _unitOfWork.OrderDetail.GetAll(
                    u => u.OrderId == orderId, includeProperties: "Product"
                )
            };
            // 將 ViewModel 傳到 View 顯示訂單內容
            return View(OrderVM);
        }

        // 點擊「立即付款」按鈕後，進行 Stripe 支付流程（POST）
        // 注意：這裡使用 ActionName("Details") 讓它對應同一個 Details 路由，但是 POST 方法
        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_PAY_NOW(int orderId)
        {
            // 重新取得 OrderHeader 資料（避免使用 ViewModel 傳遞資料被竄改）
            OrderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
                u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser"
            );

            // 重新取得該筆訂單的所有 OrderDetail 項目
            OrderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(
                    u => u.OrderId == OrderVM.OrderHeader.Id, includeProperties: "Product"
            );
            // 建立 Stripe 的付款會話選項（SessionCreateOptions）
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

            // 將訂單的每一個商品項目加入 Stripe 的付款項目清單中
            foreach (var item in OrderVM.OrderDetail)
            {
                // 建立單一商品的付款項目
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), //20.00 -> 2000
                        Currency = "usd", // 設定幣別
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title  // 商品名稱
                        },
                    },
                    Quantity = item.Count, // 商品數量
                };
                options.LineItems.Add(sessionLineItem);
            }

            // 紀錄 Stripe 明細數量到 Log
            _logger.LogInformation($"options.LineItems.Count: {options.LineItems.Count}");

            // 呼叫 Stripe 的 API 建立付款 Session
            var service = new SessionService();
            Session session = service.Create(options);

            // 更新訂單的 Stripe SessionId 與 PaymentIntentId（可用於後續追蹤付款狀態）
            _unitOfWork.OrderHeader.UpdateStripePaymentID(
                OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId
            );
            _unitOfWork.Save(); // 實際寫入資料庫

            // 將 Stripe 給的付款網址放入 HTTP Header 中
            Response.Headers.Add("Location", session.Url);

            // 回傳 303 狀態碼（See Other），讓瀏覽器重導向到 Stripe 的付款頁面
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderid)
        {
            // 根據傳入的訂單 ID 取得訂單主檔
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderid);

            // 僅在付款狀態為「延遲付款」時才檢查 Stripe 狀態（避免重複更新）
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayed)
            {
                var service = new SessionService();
                // 從 Stripe 取得目前訂單的付款會話（session）
                Session session = service.Get(orderHeader.SessionId);

                // 如果 Stripe 回傳的狀態為 "paid"，代表付款完成
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    // 紀錄付款細節
                    _logger.LogInformation($"session id: {orderHeader.SessionId ?? "Unknown"}");
                    _logger.LogInformation($"session paymentIntentId: {session.PaymentIntentId}");

                    // 更新資料庫中的 SessionId 與 PaymentIntentId（可追蹤付款）
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(
                        orderHeaderid, orderHeader.SessionId ?? "", session.PaymentIntentId
                    );
                    // 更新訂單狀態為已付款
                    _unitOfWork.OrderHeader.UpdateStatus(
                        orderHeaderid, orderHeader.OrderStatus ?? "", SD.PaymentStatusApproved
                    );
                    // 寫入變更
                    _unitOfWork.Save();
                }
            }
            // 將訂單 ID 傳到 View
            return View(orderHeaderid);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail()
        {
            // 防止 OrderVM 為 null，避免空參考例外
            if (OrderVM == null)
            {
                throw new ArgumentNullException($"OrderVM is null.");
            }
            // 從資料庫取得訂單主檔（不追蹤，避免衝突）
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(
                u => u.Id == OrderVM.OrderHeader.Id, tracked: false
            );

            // 更新收件人資訊與地址
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

            // 如果物流公司與追蹤號碼不為空，就更新
            if (OrderVM.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            // 設定最後更新時間
            orderHeaderFromDb.LastUpdateTime = DateTime.Now;

            // 寫入更新資料
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";

            // 重新導向回訂單詳細頁
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
            // 取得訂單
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(
                u => u.Id == OrderVM.OrderHeader.Id, tracked: false
            );

            // 更新訂單狀態為「處理中」
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

            // 設定物流資訊與狀態
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            // orderHeader.PaymentStatus = SD.PaymentStatusDelayed;
            // 設定狀態為「已出貨」
            orderHeader.PaymentStatus = SD.StatusShipped;
            orderHeader.OrderStatus = SD.StatusShipped;
            // 設定出貨日期
            orderHeader.ShippingDate = DateTime.Now;

            // 若為延遲付款，設定應付款日期為 30 天後
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayed)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            orderHeader.LastUpdateTime = DateTime.Now;
            // 更新與儲存
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

            // 如果訂單已付款，需透過 Stripe 退款
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);
                // 將訂單狀態更新為「已取消」+「已退款」
                _unitOfWork.OrderHeader.UpdateStatus(
                    orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded
                );
            }
            else
            {
                // 未付款，僅更新為「已取消」
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