using HoshiBook.DataAccess;
using HoshiBook.Models;
using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBookWeb.Tools;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using HoshiBook.Models.ViewModels;

namespace HoshiBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        //GET
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(
                    u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    }
                ),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(
                    u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    }
                )
            };

            if (id == null || id == 0) {
                // create product
                return View(productVM);
            }
            else
            {
                // update product
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);
            }
        }

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            try {
                if (ModelState.IsValid)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    if (file != null)
                    {
                        string filename = Guid.NewGuid().ToString();
                        string? uploads = Path.Combine(wwwRootPath, @"images\products");
                        string? extension = Path.GetExtension(file.FileName);
                        string? storagePath = Path.Combine(uploads, filename + extension);

                        // TODO If storage path does not exist, then create it.
                        FileTool.CheckAndCreateDirectory(uploads);

                        // TODO Check product image URL does exists.
                        if (obj.Product.ImageUrl != null)
                        {
                            // TODO If does exists, then obtain full storage path.
                            string? oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                            FileTool.CheckFileExistsAndRemove(oldImagePath);
                        }

                        using (var fileStreams = new FileStream(storagePath, FileMode.Create))
                        {
                            file.CopyTo(fileStreams);
                        }
                        obj.Product.ImageUrl = @"images\products\" + filename + extension;
                    }

                    if (obj.Product.Id == 0)
                    {
                        _unitOfWork.Product.Add(obj.Product);
                        _unitOfWork.Save();
                    }
                    else
                    {
                        _unitOfWork.Product.Update(obj.Product);
                        _unitOfWork.Save();
                    }
                    TempData["success"] = "Product created successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ExceptionTool.CollectDetailMessage(ex);
            }
            return View(obj);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return Json(new { data = productList });
        }

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);

            if (obj == null)
            {
                return Json(
                    new {success = false, message = "Error while deleting"}
                );
            }

            string? oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            // TODO Check product image URL does exists.
            if (obj.ImageUrl != null)
            {
                // TODO If does exists, then obtain full storage path.
                FileTool.CheckFileExistsAndRemove(oldImagePath);
            }
            
            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();

            return Json(
                new {success = true, message = "Delete Successful"}
            );
        }
        #endregion
    }
}