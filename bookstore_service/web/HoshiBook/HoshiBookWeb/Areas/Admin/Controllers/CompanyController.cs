using HoshiBook.Models;
using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBookWeb.Tools;
using HoshiBookWeb.Tools.CommonTool;


using Microsoft.AspNetCore.Mvc;
using System.Data;


namespace HoshiBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;


        public CompanyController(
            ILogger<CompanyController> logger, IUnitOfWork unitOfWork,
            IConfiguration config
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        //GET
        public IActionResult Upsert(int? id)
        {
            Company company = new();

            if (id == null || id == 0) {
                // create company
                return View(company);
            }
            else
            {
                // update company
                company = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
                return View(company);
            }
        }

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
            try {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation("CompanyController.Upsert: ModelState is valid");
                    if (obj.Id == 0)
                    {
                        _logger.LogInformation("CompanyController.Upsert: Create company {0}", obj.Id);
                        obj.CreatedAt = DateTime.Now;
                        _unitOfWork.Company.Add(obj);
                        _unitOfWork.Save();
                        TempData["success"] = "Company created successfully";
                    }
                    else
                    {
                        _logger.LogInformation("CompanyController.Upsert: Update company {0}", obj.Id);
                        Company companyFromDb = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == obj.Id);
                        companyFromDb.Name = obj.Name;
                        companyFromDb.StreetAddress = obj.StreetAddress;
                        companyFromDb.City = obj.City;
                        companyFromDb.State = obj.State;
                        companyFromDb.PostalCode = obj.PostalCode;
                        companyFromDb.PhoneNumber = obj.PhoneNumber;
                        companyFromDb.ModifiedAt = DateTime.Now;
                        _unitOfWork.Company.Update(companyFromDb);
                        _unitOfWork.Save();
                        TempData["success"] = "Company updated successfully";
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogInformation("CompanyController.Upsert: ModelState is invalid");
                    return View(obj);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("CompanyController.Upsert: {0}", ex.Message);
            }
            return View(obj);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }

        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);

            if (obj == null)
            {
                return Json(
                    new {success = false, message = "Error while deleting"}
                );
            }

            int _CompanyExistsUsersCount = _unitOfWork.Company.GetExistsUsersCompaniesCount(obj.Id);

            if (_CompanyExistsUsersCount > 0)
            {
                return Json(
                    new {
                        success = false,
                        message = $"Company has users. Cannot delete, count: {_CompanyExistsUsersCount}."
                    }
                );
            }

            _unitOfWork.Company.Remove(obj);
            _unitOfWork.Save();

            return Json(
                new {success = true, message = "Company delete Successful"}
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
                int _companyCreatedCount = 0;

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

                    List<Company> companyList = new List<Company>();

                    foreach (var sheet in Results)
                    {
                        foreach (var rows in sheet)
                        {
                            bool _allowCreateCompany = true;
                            Company company = new Company();

                            company.Name = rows["Column0"].ToString() ?? "";
                            company.StreetAddress = rows["Column1"].ToString() ?? "";
                            company.City = rows["Column2"].ToString() ?? "";
                            company.State = rows["Column3"].ToString() ?? "";
                            company.PostalCode = rows["Column4"].ToString() ?? "";
                            company.PhoneNumber = rows["Column5"].ToString() ?? "";
                            company.CreatedAt = DateTime.Now;

                            if (String.IsNullOrEmpty(company.Name))
                            {
                                throw new Exception("Name is required.");
                            }

                            if (String.IsNullOrEmpty(company.PhoneNumber))
                            {
                                throw new Exception("PhoneNumber is required.");
                            }

                            bool _NameIsExists = _unitOfWork.Company.IsExists(
                                includeProperties: "Name", company.Name
                            );
                            bool _PhoneNumberIsExists = _unitOfWork.Company.IsExists(
                                includeProperties: "PhoneNumber", company.PhoneNumber
                            );

                            if (_NameIsExists)
                            {
                                // throw new Exception("Name is exists.");
                                _allowCreateCompany = false;
                            }

                            if (_PhoneNumberIsExists)
                            {
                                // throw new Exception("PhoneNumber is exists.");
                                _allowCreateCompany = false;
                            }

                            if (_allowCreateCompany)
                            {
                                companyList.Add(company);
                                _companyCreatedCount++;
                            }

                            _logger.LogInformation(
                                "Name: {0}, StreetAddress: {1}, City: {2}, State: {3}, PostalCode: {4}, PhoneNumber: {5}",
                                company.Name, company.StreetAddress, company.City, company.State, company.PostalCode, company.PhoneNumber 
                            );
                        }
                    }
                    if (companyList.Count > 0)
                    {
                        //TODO Bulk add categories, it is faster than add one by one. don't need to save after each add.
                        _unitOfWork.Company.BulkAdd(companyList);
                    }
                }

                if (_companyCreatedCount > 0)
                {
                    _logger.LogInformation("CompanyController.BulkCreate: {0}", "Bulk create successful!");
                    return Json(
                        new {success = true, message = "Bulk create successful!"}
                    );
                }
                else
                {
                    _logger.LogInformation("CompanyController.BulkCreate: {0}", "No company created!");
                    return Json(
                        new {success = false, message = "No company created!"}
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("CompanyController.BulkCreate Message: {0}", ex.Message);
                _logger.LogError("CompanyController.BulkCreate StackTrace: {0}", ex.StackTrace);
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
                List<Company> companyList = _unitOfWork.Company.GetAll().ToList();

                if (companyList.Count == 0)
                {
                    throw new Exception("No data to export.");
                }

                DataSet ds = new DataSet();
                ds = _unitOfWork.Company.ConvertToDataSet(companyList);

                string fileName = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + "_CompaniesDetails.xlsx";

                return File(
                    FileExportTool.ExportToExcelDownload(ds),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("CompanyController.ExportDetails: {0}", ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion
    }
}