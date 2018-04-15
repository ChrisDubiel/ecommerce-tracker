namespace EcommerceTracker.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Web.Mvc;
    using AutoMapper;
    using DataAccess.Contexts;
    using Domain.Models;
    using Extensions;
    using Microsoft.Ajax.Utilities;
    using Microsoft.AspNet.Identity;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;
    using PagedList;
    using Services;
    using ViewModels;

    [Authorize]
    public class PurchasesController : Controller
    {
        private readonly AmazonFileService _amazonFileService = new AmazonFileService();
        private readonly EcommerceTrackerContext _db = new EcommerceTrackerContext();

        // GET: Purchases
        public ActionResult Index(string currentFilter, string searchBy, string sortOrder, int pageNumber = 1)
        {
            const int pageSize = 15;
            var userId = User.Identity.GetUserId();

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.CategorySortParam = sortOrder == "category_asc" ? "category_desc" : "category_asc";
            ViewBag.NecessitySortParam = sortOrder == "necessity_asc" ? "necessity_desc" : "necessity_asc";
            ViewBag.SiteSortParam = sortOrder == "site_asc" ? "site_desc" : "site_asc";
            ViewBag.PriceSortParam = sortOrder == "price_asc" ? "price_desc" : "price_test_asc";
            ViewBag.QuantitySortParam = sortOrder == "quantity_asc" ? "quantity_desc" : "quanity_asc";
            ViewBag.DateSortParam = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            // If user provides a search string, reset pageNumber
            if (searchBy != null)
                pageNumber = 1;

            // Set the search string to the current filter string
            if (currentFilter != null)
                searchBy = currentFilter;
            ViewBag.CurrentFilter = searchBy;

            // Find all purchases for user
            var purchases = _db.Purchases.Where(p => p.UserId == userId).OrderByDescending(x => x.OrderDate).ToList();

            // Filter purchases by search string
            if (!string.IsNullOrEmpty(searchBy))
                purchases = FilterPurchases(purchases, searchBy).ToList();

            // Sort purchases by sort order
            if (!string.IsNullOrEmpty(sortOrder))
                purchases = SortPurchases(purchases, sortOrder).ToList();

            // Return view of purchases
            var filteredPurchaseViewModels = Mapper.Map<List<PurchaseViewModel>>(purchases);
            return View(filteredPurchaseViewModels.ToPagedList(pageNumber, pageSize));
        }

        // GET: Purchases/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var purchase = _db.Purchases.Include(p => p.Category)
                .Include(p => p.Site).SingleOrDefault(x => x.Id == id);
            if (purchase == null)
                return HttpNotFound();

            var editPurchaseViewModel = Mapper.Map<EditPurchaseViewModel>(purchase);
            editPurchaseViewModel.Categories = GetCategories();
            editPurchaseViewModel.Sites = GetSites();
            editPurchaseViewModel.NecessityValues = GetNecessityValues();
            return View(editPurchaseViewModel);
        }

        // POST: Purchases/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPurchase(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var userId = User.Identity.GetUserId();
            var purchaseToUpdate = _db.Purchases.Where(x => x.UserId == userId).SingleOrDefault(x => x.Id == id);

            if (purchaseToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(purchaseToUpdate, "",
                new[] {"CategoryId", "UseCategoryNecessityValue", "NecessityValue"}))
                try
                {
                    _db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (DataException)
                {
                    ModelState.AddModelError("",
                        "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }

            var editPurchaseViewModel = Mapper.Map<EditPurchaseViewModel>(purchaseToUpdate);
            editPurchaseViewModel.Categories = GetCategories();
            editPurchaseViewModel.Sites = GetSites();
            editPurchaseViewModel.NecessityValues = GetNecessityValues();
            return View(editPurchaseViewModel);
        }

        // GET: Purchases/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var purchase = Mapper.Map<PurchaseViewModel>(_db.Purchases
                .Include(p => p.Category)
                .Include(p => p.Site)
                .SingleOrDefault(x => x.Id == id)
            );
            if (purchase == null)
                return HttpNotFound();
            return View(purchase);
        }

        public ActionResult ImportAmazonPurchases()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportAmazonPurchases(AmazonLoginViewModel amazonLoginViewModel)
        {
            var userId = User.Identity.GetUserId();
            var folderName = $@"C:\EcommerceTracker\AmazonFiles\{userId}";

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("download.default_directory", folderName);
            chromeOptions.AddUserProfilePreference("intl.accept_languages", "nl");
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");

            // Amazon uses a captcha to prevent headless browsers from logging in
            // chromeOptions.AddArgument("--headless");
            // TODO: Move to seperate method
            using (var driver = new ChromeDriver(chromeOptions))
            {
                try
                {
                    // Navigate to Order History Report page
                    driver.Navigate().GoToUrl("https://www.amazon.com/gp/b2b/reports");

                    // Login to Amazon, with alternate login page detection
                    // TODO: Check if username or password are incorrect
                    var passwordFormFieldExists = driver.FindElementsById("ap_password").Count > 0;
                    if (passwordFormFieldExists)
                    {
                        driver.FindElementById("ap_email").SendKeys(amazonLoginViewModel.EmailAddress);
                        driver.FindElementById("ap_password").SendKeys(amazonLoginViewModel.Password);
                        driver.FindElementById("signInSubmit").Click();
                    }
                    else
                    {
                        driver.FindElementById("ap_email").SendKeys(amazonLoginViewModel.EmailAddress);
                        driver.FindElementById("continue").Click();
                        driver.FindElementById("ap_password").SendKeys(amazonLoginViewModel.Password);
                        driver.FindElementById("signInSubmit").Click();
                    }

                    var waitForForm = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    waitForForm.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.Id("report-form")));

                    // Look for existing reports
                    // var generatedReportHistoryTableRows = driver.FindElementsByXPath("//*[@class='order-hist-submitted']/tbody/tr");

                    var startMonthSelect = new SelectElement(driver.FindElementById("report-month-start"));
                    startMonthSelect.SelectByValue("1");
                    var startDaySelect = new SelectElement(driver.FindElementById("report-day-start"));
                    startDaySelect.SelectByValue("1");
                    var startYearSelect = new SelectElement(driver.FindElementById("report-year-start"));
                    startYearSelect.SelectByIndex(startYearSelect.Options.Count - 1);
                    driver.FindElementById("report-use-today").Click();
                    driver.FindElementById("report-name").SendKeys("Generated for Ecommerce Tracker");
                    driver.FindElementById("report-confirm").Click();

                    var reportProcessed = false;
                    var reportFailed = false;
                    do
                    {
                        if (driver.FindElementsById("processing-report-table-row").Count < 1)
                            reportProcessed = true;
                        else if (driver.FindElementById("processing-long").Displayed)
                            driver.FindElementByXPath("//*[@id='report-refresh-button']/img").Click();
                        else if (driver.FindElementById("processing-failed").Displayed)
                            reportFailed = true;
                        Thread.Sleep(TimeSpan.FromSeconds(3));
                    } while (!reportProcessed && !reportFailed);
                }
                catch (Exception)
                {
                    TempData["HTML"] = driver.PageSource;
                    return RedirectToAction("SeleniumError");
                }
            }

            // TODO: Check if directory exists
            var fileNames = Directory.GetFiles(folderName);
            // TODO: Check if files exist
            var totalFileUploadResults = new FileUploadResultsViewModel
            {
                ExistingPurchases = _db.Purchases.Count(x => x.UserId == userId)
            };
            var existingPurchaseFiles = _db.TrackedPurchaseFiles.Where(x => x.UserId == userId).ToList();
            foreach (var fileName in fileNames)
            {
                if (existingPurchaseFiles.Select(x => x.FileName).Contains(fileName)) continue;
                var fileUploadResult = ImportAmazonFile(fileName);
                totalFileUploadResults.NewSites += fileUploadResult.NewSites;
                totalFileUploadResults.NewCategories += fileUploadResult.NewCategories;
                totalFileUploadResults.NewPurchases += fileUploadResult.NewPurchases;
                _db.TrackedPurchaseFiles.Add(new TrackedPurchaseFile
                {
                    UserId = userId,
                    FileName = fileName,
                    FolderName = folderName,
                    Website = "Amazon.com",
                    ImportDate = DateTime.Now
                });
            }
            _db.SaveChanges();

            return RedirectToAction("FileUploadResults", totalFileUploadResults);
        }

        public ActionResult FileUploadResults(FileUploadResultsViewModel fileUploadResultsViewModel)
        {
            return View(fileUploadResultsViewModel);
        }

        public ActionResult SeleniumError()
        {
            ViewBag.HTML = TempData["HTML"];
            return View();
        }

        private FileUploadResultsViewModel ImportAmazonFile(string fileName)
        {
            var userId = User.Identity.GetUserId();
            var existingPurchases = _db.Purchases.Where(p => p.UserId == userId).ToList();

            var newCategoryCount = new int();
            var newSiteCount = new int();
            var newPurchaseCount = new int();
            try
            {
                var reader = new StreamReader(new FileStream(fileName, FileMode.Open));
                var amazonPurchases = _amazonFileService.ExtractPurchasesFromFile(reader);
                reader.Close();
                foreach (var amazonPurchase in amazonPurchases)
                {
                    if (IsDuplicatePurchase(amazonPurchase.Website, amazonPurchase.OrderId) ||
                        amazonPurchase.Quantity == 0) continue;

                    // TODO: Note, this Linq statement is converted to SQL, which ignores case when it compares the strings 
                    var suggestedCategoryName = _db.SuggestedCategoryNames
                        .SingleOrDefault(x => x.CategoryName == amazonPurchase.Category)?.SuggestedName;

                    var category = _db.Categories.Where(x => x.UserId == userId)
                        .SingleOrDefault(c => suggestedCategoryName == null
                            ? amazonPurchase.Category.Equals(c.Name)
                            : suggestedCategoryName.Equals(c.Name));

                    // If category doesn't exist, create it
                    if (!amazonPurchase.Category.IsNullOrWhiteSpace() && category == null)
                    {
                        var categoryName = suggestedCategoryName.IsNullOrWhiteSpace()
                            ? amazonPurchase.Category
                            : suggestedCategoryName;

                        // TODO: Note, this Linq statement is converted to SQL, which ignores case when it compares the strings
                        var suggestedParentCategoryName = _db.SuggestedParentCategories
                            .SingleOrDefault(x => x.CategoryName == categoryName)?.ParentCategoryName;

                        var parentCategory = _db.Categories.Where(x => x.UserId == userId)
                            .SingleOrDefault(x => x.Name == suggestedParentCategoryName);

                        // If parent category doesn't exist, create it
                        if (!suggestedParentCategoryName.IsNullOrWhiteSpace() && parentCategory == null)
                        {
                            parentCategory = new Category
                            {
                                Name = suggestedParentCategoryName,
                                UserId = userId,
                                NecessityValue = GetSuggestedNecessityValueForCategory(suggestedParentCategoryName) ?? NecessityValue.SomewhatUnecessary,
                                UseParentCategoryNecessityValue = false
                            };
                            _db.Categories.Add(parentCategory);
                        }

                        category = new Category
                        {
                            Name = categoryName,
                            UserId = userId,
                            NecessityValue = GetSuggestedNecessityValueForCategory(categoryName) ?? NecessityValue.SomewhatUnecessary,
                            ParentCategory = parentCategory,
                            UseParentCategoryNecessityValue = true
                        };
                        newCategoryCount++;
                        _db.Categories.Add(category);
                    }

                    

                    var site = _db.Sites.SingleOrDefault(c => amazonPurchase.Website.Equals(c.Name));
                    if (site == null)
                    {
                        site = new Site
                        {
                            Name = amazonPurchase.Website
                        };
                        newSiteCount++;
                        _db.Sites.Add(site);
                    }

                    var purchase = new Purchase
                    {
                        UserId = userId,
                        Name = amazonPurchase.Title,
                        UnitPrice = decimal.Parse(amazonPurchase.PurchasePricePerUnit,
                            NumberStyles.Currency),
                        Quantity = amazonPurchase.Quantity,
                        SiteId = site.Id,
                        OrderDate = amazonPurchase.OrderDate,
                        OrderReferenceNumber = amazonPurchase.OrderId,
                        Category = category,
                        NecessityValue = NecessityValue.SomewhatUnecessary,
                        UseCategoryNecessityValue = true
                    };

                    newPurchaseCount++;
                    _db.Purchases.Add(purchase);
                    _db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var uploadFileResults = new FileUploadResultsViewModel
            {
                ExistingPurchases = existingPurchases.Count,
                NewCategories = newCategoryCount,
                NewSites = newSiteCount,
                NewPurchases = newPurchaseCount
            };
            return uploadFileResults;
        }

        /// <summary>
        ///     Retrieve all categories for the user
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SelectListItem> GetCategories()
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

            categories.AddRange(_db.Categories.Where(x => x.UserId == userId).OrderBy(x => x.Name).Select(
                x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }));

            return new SelectList(categories, "Value", "Text");
        }

        /// <summary>
        ///     Retrieve all sites
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SelectListItem> GetSites()
        {
            var sites = _db.Sites.OrderBy(x => x.Name).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            });

            return new SelectList(sites, "Value", "Text");
        }

        /// <summary>
        ///     Retrieve all necessity values
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

        /// <summary>
        ///     Filter purchases by filter parameter
        /// </summary>
        /// <param name="purchases"></param>
        /// <param name="filterParam"></param>
        /// <returns></returns>
        private static IEnumerable<Purchase> FilterPurchases(IEnumerable<Purchase> purchases, string filterParam)
        {
            var query = filterParam.ToUpper().Trim();
            purchases = purchases.Where(
                x =>
                    x.Name.ToUpper().Contains(query) ||
                    (x.Category?.Name ?? string.Empty).ToUpper().Contains(query) ||
                    x.UnitPrice.ToString(CultureInfo.InvariantCulture).ToUpper().Contains(query) ||
                    x.Quantity.ToString().ToUpper().Contains(query) ||
                    (x.Site?.Name ?? string.Empty).ToUpper().Contains(query) ||
                    x.OrderDate.Date.ToString(CultureInfo.InvariantCulture).ToUpper().Contains(query)).ToList();
            return purchases;
        }

        /// <summary>
        ///     Sort purchases by sort parameter
        /// </summary>
        /// <param name="purchases"></param>
        /// <param name="sortParam"></param>
        /// <returns></returns>
        private static IEnumerable<Purchase> SortPurchases(IEnumerable<Purchase> purchases, string sortParam)
        {
            switch (sortParam)
            {
                case "name_asc":
                    purchases = purchases.OrderBy(x => x.Name);
                    break;
                case "name_desc":
                    purchases = purchases.OrderByDescending(x => x.Name);
                    break;
                case "category_asc":
                    purchases = purchases.OrderBy(x => x.Category?.Name).ThenBy(x => x.Name);
                    break;
                case "category_desc":
                    purchases = purchases.OrderByDescending(x => x.Category?.Name).ThenBy(x => x.Name);
                    break;
                case "necessity_asc":
                    purchases = purchases.OrderBy(x => x.GetNecessityValue()).ThenBy(x => x.Name);
                    break;
                case "necessity_desc":
                    purchases = purchases.OrderByDescending(x => x.GetNecessityValue()).ThenBy(x => x.Name);
                    break;
                case "site_asc":
                    purchases = purchases.OrderBy(x => x.Site.Name).ThenBy(x => x.Name);
                    break;
                case "site_desc":
                    purchases = purchases.OrderByDescending(x => x.Site.Name).ThenBy(x => x.Name);
                    break;
                case "price_asc":
                    purchases = purchases.OrderBy(x => x.UnitPrice).ThenBy(x => x.Name);
                    break;
                case "price_desc":
                    purchases = purchases.OrderByDescending(x => x.UnitPrice).ThenBy(x => x.Name);
                    break;
                case "quantity_asc":
                    purchases = purchases.OrderBy(x => x.Quantity).ThenBy(x => x.Name);
                    break;
                case "quantity_desc":
                    purchases = purchases.OrderByDescending(x => x.Quantity).ThenBy(x => x.Name);
                    break;
                case "date_asc":
                    purchases = purchases.OrderBy(x => x.OrderDate).ThenBy(x => x.Name);
                    break;
                case "date_desc":
                    purchases = purchases.OrderByDescending(x => x.OrderDate).ThenBy(x => x.Name);
                    break;
                default:
                    purchases = purchases.OrderByDescending(x => x.OrderDate);
                    break;
            }
            return purchases;
        }

        /// <summary>
        ///     Check if purchase already exists in database
        /// </summary>
        /// <param name="website"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private bool IsDuplicatePurchase(string website, string orderId)
        {
            var userId = User.Identity.GetUserId();
            return _db.Purchases
                .Where(x => x.UserId == userId)
                .Any(x =>
                    x.Site.Name == website &&
                    x.OrderReferenceNumber == orderId);
        }

        private NecessityValue? GetSuggestedNecessityValueForCategory(string categoryName)
        {
            var suggestedNecessityValues = _db.SuggestedNecessityValues.ToList();
            var suggestedNecessityValueObject =
                _db.SuggestedNecessityValues.SingleOrDefault(x => x.CategoryName == categoryName);
            var suggestedNecessityValue = suggestedNecessityValueObject?.NecessityValue;
            return suggestedNecessityValue;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
