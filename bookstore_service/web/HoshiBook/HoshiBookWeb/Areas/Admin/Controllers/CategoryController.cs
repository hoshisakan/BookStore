using HoshiBook.Models;
using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Utility;
using HoshiBookWeb.Tools;
using HoshiBookWeb.Tools.CommonTool;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data;


namespace HoshiBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;


        public CategoryController(
            ILogger<CategoryController> logger, IUnitOfWork unitOfWork,
            IConfiguration config
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _config = config;
        }

        public ActionResult Index()
        {
            return View();
        }

        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString()) {
                //TODO ModelState.AddModelError("fieldName", "errorMessage")
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
            }
            if (ModelState.IsValid) {
                obj.CreatedAt = DateTime.Now;
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) {
                return NotFound();
            }

            var categoryFromDbFirst = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);

            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }
            return View(categoryFromDbFirst);
        }

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString()) {
                //TODO ModelState.AddModelError("fieldName", "errorMessage")
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
            }
            if (ModelState.IsValid) {
                Category categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == obj.Id);
                categoryFromDb.Name = obj.Name;
                categoryFromDb.DisplayOrder = obj.DisplayOrder;
                categoryFromDb.ModifiedAt = DateTime.Now;
                _unitOfWork.Category.Update(categoryFromDb);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var objCategoryList = _unitOfWork.Category.GetAll().ToList();
            _logger.LogInformation("Category list: {0}", objCategoryList);
            return Json(new { data = objCategoryList });
        }

        //DELETE
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);

            if (obj == null) {
                return NotFound();
            }

            int _CategoryExistsProductsCount = _unitOfWork.Category.GetExistsProductsCategoriesCount(obj.Id);

            if (_CategoryExistsProductsCount > 0)
            {
                return Json(
                    new {
                            success = false,
                            message = $"Category cannot be deleted because it is associated with a product, count: {_CategoryExistsProductsCount}."
                        }
                );
            }

            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();

            return Json(
                new {
                    success = true,
                    message = "Category deleted successfully."
                }
            );
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
                int _categoryCreatedCount = 0;

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
                    Results = FileReadTool.ReadExcelFile(filePath, false, 1);

                    if (Results.Count == 0)
                    {
                        throw new Exception("Upload file failed, because read file content failed.");
                    }

                    List<Category> categoryList = new List<Category>();

                    foreach (var sheet in Results)
                    {
                        foreach (var rows in sheet)
                        {
                            bool _allowCreateCategory = true;
                            Category category = new Category();
                            category.Name = rows["Column0"].ToString() ?? "";
                            category.DisplayOrder = Convert.ToInt32(rows["Column1"].ToString() ?? "0");
                            category.CreatedAt = DateTime.Now;

                            if (category.Name == "")
                            {
                                throw new Exception("Name is required.");
                            }

                            if (category.DisplayOrder == 0)
                            {
                                throw new Exception("DisplayOrder is required.");
                            }

                            bool _NameIsExists = _unitOfWork.Category.IsExists(
                                includeProperties: "Name", category.Name
                            );
                            bool _DisplayOrderIsExists = _unitOfWork.Category.IsExists(
                                includeProperties: "DisplayOrder", category.DisplayOrder.ToString()
                            );

                            if (_NameIsExists)
                            {
                                // throw new Exception("Name is exists.");
                                _allowCreateCategory = false;
                            }

                            if (_DisplayOrderIsExists)
                            {
                                // throw new Exception("DisplayOrder is exists.");
                                _allowCreateCategory = false;
                            }

                            if (_allowCreateCategory)
                            {
                                categoryList.Add(category);
                                _categoryCreatedCount++;
                            }

                            _logger.LogInformation(
                                "Name: {0}, DisplayOrder: {1}",
                                category.Name, category.DisplayOrder
                            );
                        }
                    }
                    if (categoryList.Count > 0)
                    {
                        //TODO Bulk add categories, it is faster than add one by one. don't need to save after each add.
                        _unitOfWork.Category.BulkAdd(categoryList);
                    }
                }

                if (_categoryCreatedCount > 0)
                {
                    _logger.LogInformation("CategoryController.BulkCreate: {0}", "Bulk create successful!");
                    return Json(
                        new {success = true, message = "Bulk create successful!"}
                    );
                }
                else
                {
                    _logger.LogInformation("CategoryController.BulkCreate: {0}", "No category created!");
                    return Json(
                        new {success = false, message = "No category created!"}
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("CategoryController.BulkCreate Message: {0}", ex.Message);
                _logger.LogError("CategoryController.BulkCreate StackTrace: {0}", ex.StackTrace);
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
                List<Category> categoryList = _unitOfWork.Category.GetAll().ToList();

                if (categoryList.Count == 0)
                {
                    throw new Exception("No data to export.");
                }

                DataSet ds = new DataSet();
                ds = _unitOfWork.Category.ConvertToDataSet(categoryList);

                string fileName = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + "_CategoriesDetails.xlsx";

                return File(
                    FileExportTool.ExportToExcelDownload(ds),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("CategoryController.ExportDetails: {0}", ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion
    }
}