namespace EcommerceTracker.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using DataAccess.Contexts;
    using Domain.Models;
    using Extensions;
    using Microsoft.AspNet.Identity;
    using ViewModels;

    [Authorize]
    public class AlertsController : Controller
    {
        private readonly EcommerceTrackerContext _db = new EcommerceTrackerContext();

        // GET: Alerts
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var alerts = _db.Alerts.Where(x => x.UserId == userId).ToList();

            var alertViewModels = new List<AlertViewModel>();
            foreach (var alert in alerts)
            {
                switch (alert.AlertType)
                {
                    case AlertType.Category:
                        var categoryAlert = (CategoryAlert) alert;
                        var currentAmountInCategories = GetCategoryOrderSum(categoryAlert.CategoryIds, alert.NumberOfMonths);
                        alertViewModels.Add(
                            new AlertViewModel
                            {
                                Id = categoryAlert.Id,
                                AlertType = categoryAlert.AlertType,
                                CategoryOrNecessityNames = GetCategoryNames(categoryAlert.CategoryIds),
                                CurrentAmount = currentAmountInCategories,
                                ThresholdAmount = categoryAlert.CostThreshold,
                                Progress = Math.Truncate(100 * currentAmountInCategories / categoryAlert.CostThreshold)});
                        break;
                    case AlertType.Necessity:
                        var necessityAlert = (NecessityAlert)alert;
                        var currentAmountWithNecessityValues = GetNecessityOrderSum(necessityAlert.NecessityValueIds, alert.NumberOfMonths);
                        alertViewModels.Add(
                            new AlertViewModel
                            {
                                Id = necessityAlert.Id,
                                AlertType = necessityAlert.AlertType,
                                CategoryOrNecessityNames = GetNecessityNames(necessityAlert.NecessityValueIds),
                                CurrentAmount = currentAmountWithNecessityValues,
                                ThresholdAmount = necessityAlert.CostThreshold,
                                Progress = Math.Truncate(100 * currentAmountWithNecessityValues / necessityAlert.CostThreshold)
                            });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return View(alertViewModels);
        }

        private List<string> GetCategoryNames(IReadOnlyCollection<int> categoryAlertCategoryIds)
        {
            var userId = User.Identity.GetUserId();
            return _db.Categories.Where(x => x.UserId == userId && categoryAlertCategoryIds.Any(id => id == x.Id))
                .Select(x => x.Name).ToList();
        }

        private static List<string> GetNecessityNames(IReadOnlyCollection<int> necessityAlertNecessityValueIds)
        {
            return ((NecessityValue[])Enum.GetValues(typeof(NecessityValue))).ToList().Where(x => necessityAlertNecessityValueIds.Any(id => id == (int)x))
                .Select(x => x.ToDescription()).ToList();
        }

        private decimal GetCategoryOrderSum(IReadOnlyCollection<int> categoryIds, int numberOfMonths)
        {
            var todaysDate = DateTime.Now;
            var firstDayOfMonth = new DateTime(todaysDate.Year, todaysDate.Month - (numberOfMonths -1), 1);
            var userId = User.Identity.GetUserId();
            var purchasesInCategory = _db.Purchases.Where(x => x.UserId == userId &&
                                                               x.OrderDate >= firstDayOfMonth)
                .Where(x => categoryIds.Any(id => id == x.CategoryId) ||
                            categoryIds.Any(id => id == x.Category.ParentCategoryId));
            return !purchasesInCategory.Any() ? 0 : purchasesInCategory.Select(x => x.Quantity * x.UnitPrice).Sum();
        }

        private decimal GetNecessityOrderSum(IReadOnlyCollection<int> necessityValueIds, int numberOfMonths)
        {
            var todaysDate = DateTime.Now;
            var firstDayOfMonth = new DateTime(todaysDate.Year, todaysDate.Month - (numberOfMonths - 1), 1);
            var userId = User.Identity.GetUserId();
            var purchasesWithNecessityValue = _db.Purchases.Where(x => x.UserId == userId &&
                                                                       x.OrderDate >= firstDayOfMonth)
                .Where(x => necessityValueIds.Any(id => id == (int)x.NecessityValue));
            return purchasesWithNecessityValue.Select(x => x.Quantity * x.UnitPrice).Sum();
        }

        // GET: Alerts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var alert = _db.Alerts.Find(id);
            if (alert == null)
            {
                return HttpNotFound();
            }
            return View(alert);
        }

        // GET: Alerts/Create
        public ActionResult Create()
        {
            var viewModel = new EditAlertViewModel
            {
                AlertTypes = GetAlertTypeSelectListItems(),
                CategoryCheckModels = GetCategoryCheckModels(),
                NecessityValueCheckModels = GetNecessityValueCheckModels()
            };

            return View(viewModel);
        }

        // POST: Alerts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AlertType,CategoryCheckModels,NecessityValueCheckModels,Amount")] EditAlertViewModel alertViewModel)
        {
            if (!ModelState.IsValid) return View(alertViewModel);

            if (alertViewModel.AlertType == AlertType.Category)
            {
                var categoryAlert = new CategoryAlert
                {
                    AlertType = alertViewModel.AlertType,
                    UserId = User.Identity.GetUserId(),
                    CategoryIds = alertViewModel.CategoryCheckModels
                        .Where(x => x.Checked).Select(x => x.Id).ToList(),
                    CostThreshold = alertViewModel.Amount,
                    NumberOfMonths = 1
                };
                _db.Alerts.Add(categoryAlert);
            }
            else
            {
                var necessityAlert = new NecessityAlert
                {
                    AlertType = alertViewModel.AlertType,
                    UserId = User.Identity.GetUserId(),
                    NecessityValueIds = alertViewModel.NecessityValueCheckModels
                        .Where(x => x.Checked).Select(x => x.Id).ToList(),
                    CostThreshold = alertViewModel.Amount,
                    NumberOfMonths = 1
                };
                _db.Alerts.Add(necessityAlert);
            }
            
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Alerts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userId = User.Identity.GetUserId();
            var alert = _db.Alerts.SingleOrDefault(x => x.UserId == userId && x.Id == id);
            if (alert == null)
                return HttpNotFound();

            EditAlertViewModel viewModel;
            if (alert.AlertType == AlertType.Category)
            {
                var categoryAlert = (CategoryAlert)alert;
                viewModel = new EditAlertViewModel
                {
                    Id = alert.Id,
                    UserId = alert.UserId,
                    AlertType = alert.AlertType,
                    AlertTypes = GetAlertTypeSelectListItems(),
                    CategoryCheckModels = GetCategoryCheckModels(categoryAlert.CategoryIds),
                    NecessityValueCheckModels = GetNecessityValueCheckModels(),
                    Amount = alert.CostThreshold
                };
            }
            else
            {
                var necessityAlert = (NecessityAlert)alert;
                viewModel = new EditAlertViewModel
                {
                    Id = alert.Id,
                    UserId = alert.UserId,
                    AlertType = alert.AlertType,
                    AlertTypes = GetAlertTypeSelectListItems(),
                    CategoryCheckModels = GetCategoryCheckModels(),
                    NecessityValueCheckModels = GetNecessityValueCheckModels(necessityAlert.NecessityValueIds),
                    Amount = alert.CostThreshold
                };
            }

            return View(viewModel);
        }

        // POST: Alerts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,AlertType,CategoryCheckModels,NecessityValueCheckModels,Amount")] EditAlertViewModel alertViewModel)
        {
            if (!ModelState.IsValid) return View(alertViewModel);

            var userId = User.Identity.GetUserId();
            var alert = _db.Alerts.SingleOrDefault(x => x.UserId == userId && x.Id == alertViewModel.Id);
            if (alert != null)
            {
                alert.CostThreshold = alertViewModel.Amount;

                if (alertViewModel.AlertType == AlertType.Category)
                {
                    var categoryAlert = (CategoryAlert)alert;
                    categoryAlert.CategoryIds = alertViewModel.CategoryCheckModels
                        .Where(x => x.Checked).Select(x => x.Id).ToList();
                }
                else
                {
                    var necessityAlert = (NecessityAlert)alert;
                    necessityAlert.NecessityValueIds = alertViewModel.NecessityValueCheckModels
                        .Where(x => x.Checked).Select(x => x.Id).ToList();
                }
            }
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Alerts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var alert = _db.Alerts.Find(id);
            if (alert == null)
            {
                return HttpNotFound();
            }

            AlertViewModel alertViewModel;
            switch (alert.AlertType)
            {
                case AlertType.Category:
                    var categoryAlert = (CategoryAlert)alert;
                    var currentAmountInCategories = GetCategoryOrderSum(categoryAlert.CategoryIds, alert.NumberOfMonths);
                    alertViewModel = new AlertViewModel
                        {
                            Id = categoryAlert.Id,
                            AlertType = categoryAlert.AlertType,
                            CategoryOrNecessityNames = GetCategoryNames(categoryAlert.CategoryIds),
                            CurrentAmount = currentAmountInCategories,
                            ThresholdAmount = categoryAlert.CostThreshold,
                            Progress = Math.Truncate(100 * currentAmountInCategories / categoryAlert.CostThreshold)
                        };
                    break;
                case AlertType.Necessity:
                    var necessityAlert = (NecessityAlert)alert;
                    var currentAmountWithNecessityValues = GetNecessityOrderSum(necessityAlert.NecessityValueIds, alert.NumberOfMonths);
                    alertViewModel = new AlertViewModel
                        {
                            Id = necessityAlert.Id,
                            AlertType = necessityAlert.AlertType,
                            CategoryOrNecessityNames = GetNecessityNames(necessityAlert.NecessityValueIds),
                            CurrentAmount = currentAmountWithNecessityValues,
                            ThresholdAmount = necessityAlert.CostThreshold,
                            Progress = Math.Truncate(100 * currentAmountWithNecessityValues / necessityAlert.CostThreshold)
                        };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return View(alertViewModel);
        }

        // POST: Alerts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var alert = _db.Alerts.Find(id);
            if (alert != null) _db.Alerts.Remove(alert);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Retrieve all categories for the user
        /// </summary>
        /// <returns></returns>
        private List<CheckModel> GetCategoryCheckModels(List<int> categoriesToCheck = null)
        {
            var userId = User.Identity.GetUserId();
            var purchases = _db.Purchases.Where(x => x.UserId == userId).ToList();

            return purchases.Where(x => x.CategoryId != null).Select(x => x.Category).Distinct()
                .Select(x => new CheckModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Checked = categoriesToCheck?.Any(id => x.Id == id) ?? false
                }).ToList();
        }

        /// <summary>
        /// Retrieve all necessity values
        /// </summary>
        /// <returns></returns>
        private static List<CheckModel> GetNecessityValueCheckModels(List<int> necessityValuesToCheck = null)
        {
            return (from NecessityValue necessityValue in Enum.GetValues(typeof(NecessityValue))
                select new CheckModel
                {
                    Id = (int)necessityValue,
                    Name = necessityValue.ToDescription(),
                    Checked = necessityValuesToCheck?.Any(id => (int)necessityValue == id) ?? false
                }).ToList();
        }

        private static IEnumerable<SelectListItem> GetAlertTypeSelectListItems()
        {
            var alerts = from AlertType alertType in Enum.GetValues(typeof(AlertType))
                select new SelectListItem
                {
                    Value = alertType.ToString(),
                    Text = alertType.ToDescription()
                };

            return new SelectList(alerts, "Value", "Text");
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
