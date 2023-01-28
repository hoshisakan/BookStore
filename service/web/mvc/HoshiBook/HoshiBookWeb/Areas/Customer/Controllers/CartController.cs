using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models.ViewModels;
using System.Security.Claims;

namespace HoshiBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public int OrderTotal { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart =_unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == claim.Value,
                    includeProperties: "Product"
                )
            };
            foreach(var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(
                    cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100
                );
                ShoppingCartVM.CartTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            // var claimsIdentity = (ClaimsIdentity)User.Identity;
            // var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // ShoppingCartVM = new ShoppingCartVM()
            // {
            //     ListCart =_unitOfWork.ShoppingCart.GetAll(
            //         u => u.ApplicationUserId == claim.Value,
            //         includeProperties: "Product"
            //     )
            // };
            // foreach(var cart in ShoppingCartVM.ListCart)
            // {
            //     cart.Price = GetPriceBasedOnQuantity(
            //         cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100
            //     );
            //     ShoppingCartVM.CartTotal += cart.Price * cart.Count;
            // }
            // return View(ShoppingCartVM);
            return View();
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