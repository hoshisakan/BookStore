using HoshiBook.DataAccess;
using HoshiBook.Models;
using HoshiBook.DataAccess.Repository.IRepository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoshiBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ActionResult Index()
        {
            Console.WriteLine("Test Cover Type.");
            List<CoverType> objCoverTypeList = _unitOfWork.CoverType.GetAll();
            return View(objCoverTypeList);
        }

        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        //TODO add ValidateAntiForgeryToken to avoid CORS attack
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
            // var CoverTypeFromDb = _unitOfWork.CoverType.Categories.FindAsync(id);
            var CoverTypeFromDbFirst = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);
            // var CoverTypeFromDbSingle = _unitOfWork.CoverType.Categories.SingleOrDefaultAsync(u => u.Id == id);
            if (CoverTypeFromDbFirst == null) {
                return NotFound();
            }
            return View(CoverTypeFromDbFirst);
        }

        //POST
        //TODO add ValidateAntiForgeryToken to avoid CORS attack
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

        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) {
                return NotFound();
            }

            var CoverTypeFromDbFirst = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);

            if (CoverTypeFromDbFirst == null) {
                return NotFound();
            }
            return View(CoverTypeFromDbFirst);
        }

        //POST
        //TODO add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);

            if (obj == null) {
                return NotFound();
            }
            _unitOfWork.CoverType.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "CoverType deleted successfully";

            return RedirectToAction("Index");
        }
    }
}