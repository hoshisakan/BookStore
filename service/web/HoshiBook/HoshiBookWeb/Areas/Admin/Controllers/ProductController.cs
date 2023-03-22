using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBookWeb.Tools;
using HoshiBook.Utility;
using HoshiBook.Models.ViewModels;
using HoshiBookWeb.Tools.CommonTool;
using HoshiBook.Models;


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Data;

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

        public ActionResult Index()
        {
            return View();
        }

        //GET
        public IActionResult Upsert(int? id)
        {
            List<Category> categoryList = _unitOfWork.Category.GetAll();
            List<CoverType> coverTypeList = _unitOfWork.CoverType.GetAll();

            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = categoryList.Select(
                    u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    }
                ),
                CoverTypeList = coverTypeList.Select(
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
                    bool _TitleIsExists = _unitOfWork.Product.IsExists(
                        includeProperties: "Title", obj.Product.Title
                    );
                    bool _ISBNIsExists = _unitOfWork.Product.IsExists(
                        includeProperties: "ISBN", obj.Product.ISBN
                    );

                    if (_TitleIsExists)
                    {
                        throw new Exception("Title is exists.");
                    }

                    if (_ISBNIsExists)
                    {
                        throw new Exception("ISBN is exists.");
                    }

                    string wwwRootPath = _hostEnvironment.WebRootPath;

                    if (file != null)
                    {
                        var _common = new Common(_config);
                        string? uploads = "";
                        string newFileName = Guid.NewGuid().ToString();
                        string oldFileName = Path.GetFileName(file.FileName);
                        string fileExtension = '.' + oldFileName.Split('.').Last();
                        string? extension = Path.GetExtension(file.FileName);
                        uploads = _common.GetProductImageStoragePath();

                        _logger.LogInformation("Image upload path: {0}", uploads);
                        _logger.LogInformation("File extension: {0}", fileExtension);
                        
                        bool _IsContainsExtension = FileUploadTool.IsContainsExtension(fileExtension, "image");
                        
                        if (!_IsContainsExtension)
                        {
                            throw new Exception("Upload file failed, because file extension is not allowed.");
                        }

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
                        //TODO Storage user upload file to server
                        bool _IsUploadSuccess = FileUploadTool.UploadImage(file, newFileName, extension, uploads);

                        if (!_IsUploadSuccess)
                        {
                            throw new Exception("Upload file failed, because save file to server local failed.");
                        }
                        obj.Product.ImageUrl = @$"{_config["StaticFiles:RequestPath"]}\images\products\" + newFileName + extension;
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
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
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

        //DELETE
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

            List<Product> productList = _unitOfWork.Product.GetExistsOrderDetailsProducts(obj.Id);
            int _ProductIsExistsUserOrder = productList.Count;

            if (_ProductIsExistsUserOrder > 0)
            {
                return Json(
                    new {
                            success = false,
                            message = $"Product is already used in order, count: {_ProductIsExistsUserOrder}."
                        }
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
                new {success = true, message = "Delete Successful!"}
            );
        }

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        public IActionResult UploadImages(List<IFormFile> uploadProductImages)
        {
            try {
                if (uploadProductImages == null)
                {
                    throw new Exception("Please select a file to upload.");
                }

                var _common = new Common(_config);
                string? uploads = "";
                uploads = _common.GetProductImageStoragePath();
                _logger.LogInformation("Image upload path: {0}", uploads);

                foreach (var file in uploadProductImages)
                {
                    string imageFileName = Path.GetFileName(file.FileName);
                    string fileExtension = '.' + imageFileName.Split('.').Last();
                    string? extension = Path.GetExtension(file.FileName);

                    _logger.LogInformation("Received Image File extension: {0}", fileExtension);
                    bool _IsContainsExtension = FileUploadTool.IsContainsExtension(fileExtension, "image");

                    if (!_IsContainsExtension)
                    {
                        throw new Exception("Upload file failed, because file extension is not allowed.");
                    }

                    // TODO If storage path does not exist, then create it.
                    FileTool.CheckAndCreateDirectory(uploads);

                    //TODO Storage user upload file to server
                    bool _IsUploadSuccess = FileUploadTool.UploadImage(file, imageFileName, uploads);

                    if (!_IsUploadSuccess)
                    {
                        throw new Exception("Upload file failed, because save file to server local failed.");
                    }
                }
                return Json(
                    new {success = true, message = "Upload images successful!"}
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("ProductController.BulkImportImages: {0}", ex.Message);
                TempData["error"] = ex.Message;
                return Json(
                    new {success = false, message = ex.Message}
                );
            }
        }
        
        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        public IActionResult BulkCreate(IFormFile uploadProductListFile)
        {
            try {
                if (uploadProductListFile == null)
                {
                    throw new Exception("Please select a file to upload.");
                }

                var _common = new Common(_config);
                string? uploads = "";
                uploads = _common.GetUploadFilesStoragePath();
                _logger.LogInformation("Document upload path: {0}", uploads);
                string newFileName = Guid.NewGuid().ToString();
                string oldFileName = Path.GetFileName(uploadProductListFile.FileName);
                string fileExtension = '.' + oldFileName.Split('.').Last();
                string? extension = Path.GetExtension(uploadProductListFile.FileName);

                _logger.LogInformation("Received Document File extension: {0}", fileExtension);
                bool _IsContainsExtension = FileUploadTool.IsContainsExtension(fileExtension, "import");

                if (!_IsContainsExtension)
                {
                    throw new Exception("Upload file failed, because file extension is not allowed.");
                }

                // TODO If storage path does not exist, then create it.
                FileTool.CheckAndCreateDirectory(uploads);

                //TODO Storage user upload file to server
                bool _IsUploadSuccess = FileUploadTool.UploadImage(uploadProductListFile, newFileName, extension, uploads);

                if (!_IsUploadSuccess)
                {
                    throw new Exception("Upload file failed, because save file to server local failed.");
                }

                List<List<Dictionary<string, object>>> Results = new List<List<Dictionary<string, object>>>();

                string? filePath = Path.Combine(uploads, newFileName + extension);

                if (filePath != null)
                {
                    Results = FileReadTool.ReadExcelFile(filePath, false, 3);

                    if (Results.Count == 0)
                    {
                        throw new Exception("Upload file failed, because read file content failed.");
                    }

                    List<Product> productList = new List<Product>();

                    foreach (var sheet in Results)
                    {
                        foreach (var rows in sheet)
                        {
                            Product product = new Product();
                            product.Title = rows["Column1"].ToString() ?? "";
                            product.Description = rows["Column2"].ToString() ?? "";
                            product.ISBN = rows["Column3"].ToString() ?? "";
                            product.Author = rows["Column4"].ToString() ?? "";
                            product.ListPrice = Convert.ToDouble(rows["Column5"].ToString() ?? "0");
                            product.Price = Convert.ToDouble(rows["Column6"].ToString() ?? "0");
                            product.Price50 = Convert.ToDouble(rows["Column7"].ToString() ?? "0");
                            product.Price100 = Convert.ToDouble(rows["Column8"].ToString() ?? "0");
                            product.ImageUrl = @$"{_config["StaticFiles:RequestPath"]}\images\products\" + rows["Column9"].ToString() ?? "";
                            string Category = rows["Column10"].ToString() ?? "";
                            string CoverType = rows["Column11"].ToString() ?? "";

                            if (product.Title == "")
                            {
                                throw new Exception("Title is required.");
                            }

                            if (product.Description == "")
                            {
                                throw new Exception("Description is required.");
                            }

                            if (product.ISBN == "")
                            {
                                throw new Exception("ISBN is required.");
                            }

                            if (product.Author == "")
                            {
                                throw new Exception("Author is required.");
                            }

                            if (Category != "")
                            {
                                var category = _unitOfWork.Category.GetFirstOrDefault(c => c.Name == Category);
                                if (category != null)
                                {
                                    product.CategoryId = category.Id;
                                }
                                else
                                {
                                    throw new Exception("Category does not exists.");
                                }
                            }
                            else
                            {
                                throw new Exception("Category is required.");
                            }

                            if (CoverType != "")
                            {
                                var coverType = _unitOfWork.CoverType.GetFirstOrDefault(c => c.Name == CoverType);
                                if (coverType != null)
                                {
                                    product.CoverTypeId = coverType.Id;
                                }
                                else
                                {
                                    throw new Exception("CoverType does not exists.");
                                }
                            }
                            else
                            {
                                throw new Exception("CoverType is required.");
                            }

                            bool _TitleIsExists = _unitOfWork.Product.IsExists(
                                includeProperties: "Title", product.Title
                            );
                            bool _ISBNIsExists = _unitOfWork.Product.IsExists(
                                includeProperties: "ISBN", product.ISBN
                            );

                            if (_TitleIsExists)
                            {
                                throw new Exception("Title is exists.");
                            }

                            if (_ISBNIsExists)
                            {
                                throw new Exception("ISBN is exists.");
                            }

                            productList.Add(product);

                            _logger.LogInformation(
                                "Title: {0}, Description: {1}, Price: {2}, CoverTypeId: {3}, CategoryId: {4}, ImageUrl: {5}",
                                product.Title, product.Description, product.Price,
                                product.CoverTypeId, product.CategoryId, product.ImageUrl
                            );
                        }
                    }
                    //TODO Bulk add products, it is faster than add one by one. don't need to save after each add.
                    _unitOfWork.Product.BulkAdd(productList);
                }

                _logger.LogInformation("ProductController.BulkCreate: {0}", "Bulk create successful!");
                return Json(
                    new {success = true, message = "Bulk create successful!"}
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("ProductController.BulkCreate Message: {0}", ex.Message);
                _logger.LogError("ProductController.BulkCreate StackTrace: {0}", ex.StackTrace);
                return Json(
                    new {success = false, message = ex.Message}
                );
            }
        }
        
        [HttpGet]
        public IActionResult ExportDetails()
        {
            try
            {
                List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType").ToList();

                if (productList.Count == 0)
                {
                    throw new Exception("No data to export.");
                }

                DataSet ds = new DataSet();
                ds = _unitOfWork.Product.ConvertToDataSet(productList);

                string fileName = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "ProductDetails.xlsx";
                // return Json(
                //     new {success = true, message = "Export successful!"}
                // );
                return File(
                    FileExportTool.ExportToExcelDownload(ds),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("ProductController.ExportDetails: {0}", ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion
    }
}