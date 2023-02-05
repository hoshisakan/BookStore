using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models;
using HoshiBook.Utility;
using HoshiBook.Models.ViewModels;



namespace HoshiBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                OrderDetail = _unitOfWork.OrderDetail.GetAll(
                    u => u.OrderId == orderId, includeProperties: "Product"
                )
            };
            return View(OrderVM);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            List<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity?)User.Identity;
                var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrderHeader.GetAll(
                    u => u.ApplicationUserId == claim.Value,
                    includeProperties: "ApplicationUser"
                );
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(
                        u => u.PaymentStatus == SD.PaymentStatusDelayed
                    ).ToList();
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(
                        u => u.PaymentStatus == SD.StatusInProgress
                    ).ToList();
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(
                        u => u.PaymentStatus == SD.StatusShipped
                    ).ToList();
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(
                        u => u.PaymentStatus == SD.StatusApproved
                    ).ToList();
                    break;
                default:
                    break;
            }
            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}