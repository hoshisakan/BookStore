using HoshiBook.DataAccess;
using HoshiBook.Models;
using HoshiBook.DataAccess.Repository.IRepository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoshiBookWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _db;

        public CategoryController(ICategoryRepository db)
        {
            _db = db;
        }

        public ActionResult Index()
        {
            List<Category> objCategoryList = _db.GetAll();
            return View(objCategoryList);
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
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString()) {
                //TODO ModelState.AddModelError("fieldName", "errorMessage")
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
            }
            if (ModelState.IsValid) {
                _db.Add(obj);
                _db.Save();
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
            // var categoryFromDb = _db.Categories.FindAsync(id);
            var categoryFromDbFirst = _db.GetFirstOrDefault(u => u.Id == id);
            // var categoryFromDbFirst = _db.Categories.FirstOrDefaultAsync(u => u.Id == id);
            // var categoryFromDbSingle = _db.Categories.SingleOrDefaultAsync(u => u.Id == id);
            if (categoryFromDbFirst == null) {
                return NotFound();
            }
            return View(categoryFromDbFirst);
        }

        //POST
        //TODO add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString()) {
                //TODO ModelState.AddModelError("fieldName", "errorMessage")
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
            }
            if (ModelState.IsValid) {
                _db.Update(obj);
                _db.Save();
                TempData["success"] = "Category updated successfully";
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
            // var categoryFromDb = _db.Categories.FindAsync(id);
            var categoryFromDbFirst = _db.GetFirstOrDefault(u => u.Id == id);
            // var categoryFromDbSingle = _db.Categories.SingleOrDefaultAsync(u => u.Id == id);
            if (categoryFromDbFirst == null) {
                return NotFound();
            }
            return View(categoryFromDbFirst);
        }

        //POST
        //TODO add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _db.GetFirstOrDefault(u => u.Id == id);

            if (obj == null) {
                return NotFound();
            }
            _db.Remove(obj);
            _db.Save();
            TempData["success"] = "Category deleted successfully";

            return RedirectToAction("Index");
        }
    }
}