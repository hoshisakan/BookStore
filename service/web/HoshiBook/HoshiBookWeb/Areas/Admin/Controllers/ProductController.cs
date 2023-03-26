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
                if (productVM.Product == null)
                {
                    return NotFound();
                }
    
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
                        includeProperties: "Title", obj.Product.Title, obj.Product.Id
                    );
                    bool _SKUIsExists = _unitOfWork.Product.IsExists(
                        includeProperties: "SKU", obj.Product.SKU, obj.Product.Id
                    );
                    bool _ISBNIsExists = _unitOfWork.Product.IsExists(
                        includeProperties: "ISBN", obj.Product.ISBN, obj.Product.Id
                    );

                    if (_TitleIsExists)
                    {
                        throw new Exception("Title is exists.");
                    }

                    if (_SKUIsExists)
                    {
                        throw new Exception("SKU is exists.");
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
                        obj.Product.CreatedAt = DateTime.Now;
                        _unitOfWork.Product.Add(obj.Product);
                        _unitOfWork.Save();
                    }
                    else
                    {
                        ProductVM productVM = new();
                        productVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == obj.Product.Id);
                        productVM.Product.Title = obj.Product.Title;
                        productVM.Product.SKU = obj.Product.SKU;
                        productVM.Product.Description = obj.Product.Description;
                        productVM.Product.ISBN = obj.Product.ISBN;
                        productVM.Product.Author = obj.Product.Author;
                        productVM.Product.ListPrice = obj.Product.ListPrice;
                        productVM.Product.Price = obj.Product.Price;
                        productVM.Product.Price50 = obj.Product.Price50;
                        productVM.Product.Price100 = obj.Product.Price100;
                        if (obj.Product.ImageUrl != null)
                        {
                            productVM.Product.ImageUrl = obj.Product.ImageUrl;
                        }
                        productVM.Product.CategoryId = obj.Product.CategoryId;
                        productVM.Product.CoverTypeId = obj.Product.CoverTypeId;
                        productVM.Product.ModifiedAt = DateTime.Now;
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

            int _ProductExistsUserOrderCount = _unitOfWork.Product.GetExistsOrderDetailsProductsCount(obj.Id);

            if (_ProductExistsUserOrderCount > 0)
            {
                return Json(
                    new {
                            success = false,
                            message = $"Product cannot be deleted because it is associated with a user order, count: {_ProductExistsUserOrderCount}."
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
        public IActionResult BulkCreate(IFormFile uploadFile)
        {
            try {
                if (uploadFile == null)
                {
                    throw new Exception("Please select a file to upload.");
                }

                var _common = new Common(_config);
                string? uploads = "";
                uploads = _common.GetUploadFilesStoragePath();
                _logger.LogInformation("Document upload path: {0}", uploads);
                string newFileName = Guid.NewGuid().ToString();
                string oldFileName = Path.GetFileName(uploadFile.FileName);
                string fileExtension = '.' + oldFileName.Split('.').Last();
                string? extension = Path.GetExtension(uploadFile.FileName);
                int _productCreatedCount = 0;

                _logger.LogInformation("Received Document File extension: {0}", fileExtension);
                bool _IsContainsExtension = FileUploadTool.IsContainsExtension(fileExtension, "import");

                if (!_IsContainsExtension)
                {
                    throw new Exception("Upload file failed, because file extension is not allowed.");
                }

                // TODO If storage path does not exist, then create it.
                FileTool.CheckAndCreateDirectory(uploads);

                //TODO Storage user upload file to server
                bool _IsUploadSuccess = FileUploadTool.UploadImage(uploadFile, newFileName, extension, uploads);

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
                            bool _allowCreateProduct = true;
                            Product product = new Product();
                            
                            product.Title = rows["Column0"].ToString() ?? "";
                            product.SKU = rows["Column1"].ToString() ?? "";
                            product.Description = rows["Column2"].ToString() ?? "";
                            product.ISBN = rows["Column3"].ToString() ?? "";
                            product.Author = rows["Column4"].ToString() ?? "";
                            product.ListPrice = Convert.ToDouble(rows["Column5"].ToString() ?? "0");
                            product.Price = Convert.ToDouble(rows["Column6"].ToString() ?? "0");
                            product.Price50 = Convert.ToDouble(rows["Column7"].ToString() ?? "0");
                            product.Price100 = Convert.ToDouble(rows["Column8"].ToString() ?? "0");
                            product.ImageUrl = @$"{_config["StaticFiles:RequestPath"]}\images\products\" + rows["Column9"].ToString() ?? "";
                            product.CreatedAt = DateTime.Now;
                            string Category = rows["Column10"].ToString() ?? "";
                            string CoverType = rows["Column11"].ToString() ?? "";

                            if (product.Title == "")
                            {
                                throw new Exception("Title is required.");
                            }

                            if (product.SKU == "")
                            {
                                throw new Exception("SKU is required.");
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
                            bool _SKUIsExists = _unitOfWork.Product.IsExists(
                                includeProperties: "SKU", product.SKU
                            );
                            bool _ISBNIsExists = _unitOfWork.Product.IsExists(
                                includeProperties: "ISBN", product.ISBN
                            );

                            if (_TitleIsExists)
                            {
                                // throw new Exception("Title is exists.");
                                _logger.LogInformation("Title is exists.");
                                _allowCreateProduct = false;
                            }

                            if (_SKUIsExists)
                            {
                                // throw new Exception("SKU is exists.");
                                _logger.LogInformation("SKU is exists.");
                                _allowCreateProduct = false;
                            }

                            if (_ISBNIsExists)
                            {
                                // throw new Exception("ISBN is exists.");
                                _logger.LogInformation("ISBN is exists.");
                                _allowCreateProduct = false;
                            }

                            if (_allowCreateProduct)
                            {
                                productList.Add(product);
                                _productCreatedCount++;
                            }

                            _logger.LogInformation(
                                "Title: {0}, SKU: {1} ,Description: {2}, Price: {3}, CoverTypeId: {4}, CategoryId: {5}, ImageUrl: {6}",
                                product.Title, product.SKU ,product.Description, product.Price,
                                product.CoverTypeId, product.CategoryId, product.ImageUrl
                            );
                        }
                    }
                    if (productList.Count > 0)
                    {
                        //TODO Bulk add products, it is faster than add one by one. don't need to save after each add.
                        _unitOfWork.Product.BulkAdd(productList);
                    }
                }

                if (_productCreatedCount > 0)
                {
                    _logger.LogInformation("ProductController.BulkCreate: {0}", "Bulk create successful!");
                    return Json(
                        new {success = true, message = "Bulk create successful!"}
                    );
                }
                else
                {
                    _logger.LogInformation("ProductController.BulkCreate: {0}", "No product created!");
                    return Json(
                        new {success = false, message = "No product created!"}
                    );
                }
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

                string fileName = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + "_ProductDetails.xlsx";
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