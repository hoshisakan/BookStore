using HoshiBook.Models;

using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBookWeb.Tools;
using Microsoft.AspNetCore.Mvc;


namespace HoshiBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(ILogger<CompanyController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
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
                    if (obj.Id == 0)
                    {
                        _unitOfWork.Company.Add(obj);
                        _unitOfWork.Save();
                        TempData["success"] = "Company created successfully";
                    }
                    else
                    {
                        _unitOfWork.Company.Update(obj);
                        _unitOfWork.Save();
                        TempData["success"] = "Company updated successfully";
                    }
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
            var companyList = _unitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }

        //POST
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
            _unitOfWork.Company.Remove(obj);
            _unitOfWork.Save();

            return Json(
                new {success = true, message = "Delete Successful"}
            );
        }
        #endregion
    }
}