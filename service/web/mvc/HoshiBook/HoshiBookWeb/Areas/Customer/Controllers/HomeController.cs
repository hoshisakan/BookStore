﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using HoshiBook.Models;
using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HoshiBook.Utility;

namespace HoshiBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return View(productList);
        }

        public IActionResult Details(int? productId)
        {
            if (productId == null)
            {
                return NotFound();
            }
            ShoppingCart cardObj = new ()
            {
                Count = 1,
                ProductId = productId.Value,
                Product = _unitOfWork.Product.GetFirstOrDefault(
                            u => u.Id == productId,
                            includeProperties: "Category,CoverType"
                        )
            };
            return View(cardObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId
            );

            if (cartFromDb == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                // int count = _unitOfWork.ShoppingCart.GetAll(
                //     u => u.ApplicationUserId == claim.Value
                // ).ToList().Count();
                int count = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == claim.Value
                ).Select(u => u.Count).Sum();
                Console.WriteLine($"The user {claim.Value} has {count} items in the cart.");
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }
            else
            {
                _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
                _unitOfWork.Save();
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
