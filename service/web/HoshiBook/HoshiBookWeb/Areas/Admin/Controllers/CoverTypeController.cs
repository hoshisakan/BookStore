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
    public class CoverTypeController : Controller
    {
        private readonly ILogger<CoverTypeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;


        public CoverTypeController(
            ILogger<CoverTypeController> logger, IUnitOfWork unitOfWork,
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
        public IActionResult Create(CoverType obj)
        {
            if (ModelState.IsValid) {
                _unitOfWork.CoverType.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "CoverType created successfully";
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

            var coverTypeFromDbFirst = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);

            if (coverTypeFromDbFirst == null) {
                return NotFound();
            }
            return View(coverTypeFromDbFirst);
        }

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            if (ModelState.IsValid) {
                _unitOfWork.CoverType.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "CoverType updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var objCoverTypeList = _unitOfWork.CoverType.GetAll();
            return Json(new { data = objCoverTypeList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);

            if (obj == null) {
                return Json(new { success = false, message = "Error while deleting" });
            }

            int _CoverTypeExistsProductCount = _unitOfWork.CoverType.GetExistsProductsCoverTypesCount(id);

            if (_CoverTypeExistsProductCount > 0) {
                return Json(
                    new {
                        success = false,
                        message = $"CoverType cannot be deleted because it is associated with a product, count: {_CoverTypeExistsProductCount}"
                    }
                );
            }

            _unitOfWork.CoverType.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "CoverType deleted successfully" });
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

                    List<CoverType> coverTypeList = new List<CoverType>();

                    foreach (var sheet in Results)
                    {
                        foreach (var rows in sheet)
                        {
                            CoverType coverType = new CoverType();
                            coverType.Name = rows["Column0"].ToString() ?? "";

                            if (coverType.Name == "")
                            {
                                throw new Exception("Name is required.");
                            }

                            bool _NameIsExists = _unitOfWork.CoverType.IsExists(
                                includeProperties: "Name", coverType.Name
                            );

                            if (_NameIsExists)
                            {
                                throw new Exception("Name is exists.");
                            }

                            coverTypeList.Add(coverType);

                            _logger.LogInformation("Name: {0}", coverType.Name);
                        }
                    }
                    //TODO Bulk add categories, it is faster than add one by one. don't need to save after each add.
                    _unitOfWork.CoverType.BulkAdd(coverTypeList);
                }

                _logger.LogInformation("CoverTypeController.BulkCreate: {0}", "Bulk create successful!");
                return Json(
                    new {success = true, message = "Bulk create successful!"}
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("CoverTypeController.BulkCreate Message: {0}", ex.Message);
                _logger.LogError("CoverTypeController.BulkCreate StackTrace: {0}", ex.StackTrace);
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
                List<CoverType> coverTypeList = _unitOfWork.CoverType.GetAll().ToList();

                if (coverTypeList.Count == 0)
                {
                    throw new Exception("No data to export.");
                }

                DataSet ds = new DataSet();
                ds = _unitOfWork.CoverType.ConvertToDataSet(coverTypeList);

                string fileName = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + "_CoverTypesDetails.xlsx";

                return File(
                    FileExportTool.ExportToExcelDownload(ds),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("CoverTypeController.ExportDetails: {0}", ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion
    }
}