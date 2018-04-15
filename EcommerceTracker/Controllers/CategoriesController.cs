namespace EcommerceTracker.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using AutoMapper;
    using DataAccess.Contexts;
    using Domain.Models;
    using Extensions;
    using Microsoft.AspNet.Identity;
    using ViewModels;

    public class CategoriesController : Controller
    {
        private readonly EcommerceTrackerContext _db = new EcommerceTrackerContext();

        // GET: Categories
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var userCategories = _db.Categories.Where(x => x.UserId == userId).ToList();
            var userCategoryViewModels = Mapper.Map<List<CategoryViewModel>>(userCategories);
            return View(userCategoryViewModels);
        }

        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userId = User.Identity.GetUserId();
            var category = _db.Categories.Where(x => x.UserId == userId).SingleOrDefault(x => x.Id == id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            var viewModel = new EditCategoryViewModel
            {
                NecessityValue = NecessityValue.SomewhatUnecessary,
                ParentCategories = GetParentCategories(),
                NecessityValues = GetNecessityValues()
            };
            return View(viewModel);
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name, ParentCategoryId, UseParentCategoryNecessityValue, NecessityValue")] EditCategoryViewModel categoryViewModel)
        {
            if (!ModelState.IsValid) return View(categoryViewModel);
            var category = Mapper.Map<Category>(categoryViewModel);
            category.UserId = User.Identity.GetUserId();
            _db.Categories.Add(category);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var userId = User.Identity.GetUserId();
            var category = _db.Categories.Where(x => x.UserId == userId).SingleOrDefault(x => x.Id == id);
            if (category == null)
                return HttpNotFound();

            var editCategoryViewModel = Mapper.Map<EditCategoryViewModel>(category);
            editCategoryViewModel.ParentCategories = GetParentCategories();
            editCategoryViewModel.NecessityValues = GetNecessityValues();

            return View(editCategoryViewModel);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        // https://docs.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/implementing-basic-crud-functionality-with-the-entity-framework-in-asp-net-mvc-application
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var userId = User.Identity.GetUserId();
            var categoryToUpdate = _db.Categories.Where(x => x.UserId == userId).SingleOrDefault(x => x.Id == id);

            if (categoryToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(categoryToUpdate, "", new[] {"Name", "ParentCategoryId", "UseParentCategoryNecessityValue", "NecessityValue"}))
                try
                {
                    _db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (DataException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("",
                        "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }

            var editCategoryViewModel = Mapper.Map<EditCategoryViewModel>(categoryToUpdate);
            editCategoryViewModel.ParentCategories = GetParentCategories();
            editCategoryViewModel.NecessityValues = GetNecessityValues();
            return View(editCategoryViewModel);
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userId = User.Identity.GetUserId();
            var category = _db.Categories.Where(x => x.UserId == userId).SingleOrDefault(x => x.Id == id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var userId = User.Identity.GetUserId();
            var category = _db.Categories.Where(x => x.UserId == userId).SingleOrDefault(x => x.Id == id);
            // TODO: redirect to error page if category not found
            if (category == null) return RedirectToAction("Index");
            _db.Categories.Remove(category);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Retrieve all categories for the user
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SelectListItem> GetParentCategories()
        {
            var userId = User.Identity.GetUserId();
            var categories = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = null,
                    Text = ""
                }
            };

            // Prevent using categories that already have a parent category (preventing grand-parent categories)
            categories.AddRange(_db.Categories.Where(x => 
                x.UserId == userId && 
                x.ParentCategoryId == null).OrderBy(x => x.Name).Select(
                    x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    }));

            return new SelectList(categories, "Value", "Text");
        }

        /// <summary>
        /// Retrieve all necessity values
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<SelectListItem> GetNecessityValues()
        {
            var necessityValues = (from NecessityValue necessityValue in Enum.GetValues(typeof(NecessityValue))
                select new SelectListItem
                {
                    Value = necessityValue.ToString(),
                    Text = necessityValue.ToDescription()
                }).ToList();

            return new SelectList(necessityValues, "Value", "Text");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
