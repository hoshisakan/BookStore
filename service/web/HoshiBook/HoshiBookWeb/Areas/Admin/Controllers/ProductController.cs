using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBookWeb.Tools;
using HoshiBook.Utility;
using HoshiBook.Models.ViewModels;
using HoshiBookWeb.Tools.CommonTool;


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace HoshiBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConfiguration _config;

        public ProductController(
            ILogger<ProductController> logger, IUnitOfWork unitOfWork,
            IWebHostEnvironment hostEnvironment, IConfiguration config
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _config = config;
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
                        var _common = new Common(_config);
                        string? uploads = "";
                        string filename = Guid.NewGuid().ToString();

                        uploads = _common.GetProductImageStoragePath();
                        _logger.LogInformation("uploads: {0}", uploads);

                        string? extension = Path.GetExtension(file.FileName);
                        string? storagePath = Path.Combine(uploads, filename + extension);
                        _logger.LogInformation("storagePath: {0}", storagePath);

                        // TODO If storage path does not exist, then create it.
                        FileTool.CheckAndCreateDirectory(uploads);

                        // TODO Check product image URL does exists.
                        if (obj.Product.ImageUrl != null)
                        {
                            // TODO If does exists, then obtain full storage path.
                            string? oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                            _logger.LogInformation("oldImagePath: {0}", oldImagePath);
                            FileTool.CheckFileExistsAndRemove(oldImagePath);
                        }

                        using (var fileStreams = new FileStream(storagePath, FileMode.Create))
                        {
                            file.CopyTo(fileStreams);
                        }
                        obj.Product.ImageUrl = @"staticfiles\images\products\" + filename + extension;
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
                _logger.LogError("ProductController.Upsert: {0}", ex.Message);
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