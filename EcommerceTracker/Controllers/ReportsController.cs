namespace EcommerceTracker.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;
    using DataAccess.Contexts;
    using Domain.Models;
    using Extensions;
    using MathNet.Numerics;
    using Microsoft.Ajax.Utilities;
    using Microsoft.AspNet.Identity;
    using ViewModels;

    [Authorize]
    public class ReportsController : Controller
    {
        private readonly EcommerceTrackerContext _db = new EcommerceTrackerContext();

        // GET: Reports
        public ActionResult Index(string reportName, string startDateFilter, string endDateFilter,
            List<CheckModel> categoryCheckModels, List<CheckModel> necessityValueCheckModels)
        {
            var purchases = GetPurchases().ToList();
            if (!purchases.Any())
                return View();
            
            var reportType = ParseReportType(reportName);
            var startDate = ParseStartDate(startDateFilter);
            var endDate = ParseEndDate(endDateFilter);
            var categoriesToFilter = ParseCategoriesToFilter(purchases, categoryCheckModels);
            var necessityValuesToFilter = ParseNecessityValuesToFilter(purchases, necessityValueCheckModels);

            var reportViewModel = GetReportViewModel(reportType, purchases, startDate, endDate, 
                categoriesToFilter, necessityValuesToFilter);
            return View(reportViewModel);
        }

        private IEnumerable<Purchase> GetPurchases()
        {
            var userID = User.Identity.GetUserId();
            return _db.Purchases.Where(x => x.UserId == userID).ToList();
        }

        private static ReportType ParseReportType(string reportName = null)
        {   
            if (!reportName.IsNullOrWhiteSpace() && Enum.TryParse(reportName, out ReportType parsedReportType))
                return parsedReportType;
            return ReportType.CategoryPercentage;
        }

        private static DateTime? ParseStartDate(string startDateFilter)
        {
            return string.IsNullOrEmpty(startDateFilter) ? (DateTime?)null : DateTime.Parse(startDateFilter);
        }

        private static DateTime? ParseEndDate(string endDateFilter)
        {
            return string.IsNullOrEmpty(endDateFilter) ? (DateTime?)null : DateTime.Parse(endDateFilter);
        }

        private static IList<Category> ParseCategoriesToFilter(IEnumerable<Purchase> purchases, IEnumerable<CheckModel> categoryCheckModels)
        {
            return categoryCheckModels == null
                ? null
                : purchases.Where(x => x.CategoryId != null)
                    .Select(x => x.Category).Distinct().Where(x => categoryCheckModels.Where(y => !y.Checked)
                        .Select(y => y.Id).Contains(x.Id)).ToList();
        }

        private static IList<NecessityValue> ParseNecessityValuesToFilter(IEnumerable<Purchase> purchases, IEnumerable<CheckModel> necessityValueCheckModels)
        {
            return necessityValueCheckModels == null
                ? null
                : purchases
                    .Select(x => x.GetNecessityValue()).Distinct().Where(x => necessityValueCheckModels.Where(y => !y.Checked)
                        .Select(y => y.Id).Contains((int)x)).ToList();
        }

        private static ReportViewModel GetReportViewModel(ReportType reportType, IList<Purchase> purchases, DateTime? startDate, DateTime? endDate,
            ICollection<Category> categoriesToFilter, ICollection<NecessityValue> necessityValuesToFilter)
        {
            var oldestPurchase = purchases.OrderBy(x => x.OrderDate).First();
            var newestPurchase = purchases.OrderByDescending(x => x.OrderDate).First();

            var filteredPurchases = FilterPurchases(purchases, startDate, endDate, categoriesToFilter, necessityValuesToFilter);
            var chart = GetChart(reportType, filteredPurchases);

            var categoryCheckModels = purchases.Where(x => x.CategoryId != null).Select(x => x.Category).Distinct()
                .Select(x => new CheckModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Checked = !categoriesToFilter?.Select(y => y.Id).Contains(x.Id) ?? true
                }).ToList();

            var necessityValueCheckModels = purchases.Select(x => x.GetNecessityValue()).Distinct()
                .Select(x => new CheckModel
                {
                    Id = (int) x,
                    Name = x.ToDescription(),
                    Checked = !necessityValuesToFilter?.Select(y => (int) y).Contains((int) x) ?? true
                }).ToList();
           
            return new ReportViewModel
            {
                ReportType = reportType,
                Chart = chart,
                OldestOrderDate = oldestPurchase.OrderDate.ToShortDateString(),
                NewestOrderDate = newestPurchase.OrderDate.ToShortDateString(),
                StartDateFilter = startDate?.ToShortDateString() ?? oldestPurchase.OrderDate.ToShortDateString(),
                EndDateFilter = endDate?.ToShortDateString() ?? newestPurchase.OrderDate.ToShortDateString(),
                ShowCategoryFilter = reportType == ReportType.CategoryPercentage || reportType == ReportType.PurchasesByCategory,
                ShowNecessityValueFilter = reportType == ReportType.PurchasesByNecessityValue,
                CategoryCheckModels = categoryCheckModels,
                NecessityValueCheckModels = necessityValueCheckModels
            };
        }

        private static IList<Purchase> FilterPurchases(IList<Purchase> purchases, DateTime? startDate, DateTime? endDate, 
            ICollection<Category> categoriesToFilter, ICollection<NecessityValue> necessityValuesToFilter)
        {
            var filteredPurchases = purchases;
            if (startDate != null)
                filteredPurchases = filteredPurchases.Where(x => x.OrderDate >= startDate).ToList();
            if (endDate != null)
                filteredPurchases = filteredPurchases.Where(x => x.OrderDate <= endDate).ToList();
            if (categoriesToFilter != null)
                filteredPurchases = filteredPurchases.Where(x => x.CategoryId != null && !categoriesToFilter.Select(y => y.Id).ToList()
                    .Contains((int)x.CategoryId)).ToList();
            if (necessityValuesToFilter != null)
                filteredPurchases = filteredPurchases.Where(x => !necessityValuesToFilter
                    .Contains(x.GetNecessityValue())).ToList();
            return filteredPurchases;
        }

        private static Chart GetChart(ReportType reportType, ICollection<Purchase> purchases)
        {
            switch (reportType)
            {
                case ReportType.CategoryPercentage:
                    return GetCategoryPercentageChart(purchases);
                case ReportType.PurchasesByCategory:
                    return GetPurchasesByCategoryChart(purchases);
                case ReportType.PurchasesByNecessityValue:
                    return GetPurchasesByNecessityValueChart(purchases);
                default:
                    return GetCategoryPercentageChart(purchases);
            }
        }

        private static Chart GetCategoryPercentageChart(ICollection<Purchase> purchases)
        {
            var chart = new Chart {Layout = {Title = "Categories by Percentage"}};

            var cutoffValue = purchases.Select(x => x.UnitPrice * x.Quantity).Sum() * (decimal) .01;

            // TODO: Capture purchases with null category
            var categories = purchases.Where(x => x.CategoryId != null).Select(x => x.Category).Distinct().ToList();
            var data = new PieData {Type = "pie"};
            foreach (var category in categories)
            {
                var purchasesInCategory = purchases.Where(x => x.CategoryId == category.Id);

                var total = purchasesInCategory.Select(x => x.UnitPrice * x.Quantity).Sum();

                // Only include categories if more than 1% of total purchases
                // TODO: This will show a skewed pie chart, since it doesn't include the low percentage cartegories
                if (total < cutoffValue) continue;
                data.Labels.Add(category.Name);
                data.Values.Add(total);
            }
            chart.Data.Add(data);
            return chart;
        }

        private static Chart GetPurchasesByCategoryChart(ICollection<Purchase> purchases)
        {
            var chart = new Chart {Layout = {Title = "Purchases by Category"}};

            // Create total purchase line
            var totalPurchaseRegressionTraceData = new TraceData {
                Name = "Total purchase regression",
                Type = "scatter",
                Mode = "lines",
                Line = { Dash = "dot", Width = 4 }
            };
            var uniqueOrderDates = purchases.Select(x => x.OrderDate.Date).Distinct().ToList();
            var xdata = new List<double>();
            var ydata = new List<double>();
            var oldestPurchaseDate = purchases.OrderBy(x => x.OrderDate).First().OrderDate;
            foreach (var uniqueOrderDate in uniqueOrderDates)
            {
                var purchaseTotal =
                    purchases.Where(x => x.OrderDate.Date == uniqueOrderDate)
                        .Select(x => x.UnitPrice * x.Quantity).Sum();
                var timespan = uniqueOrderDate - oldestPurchaseDate;
                xdata.Add(timespan.TotalHours);
                ydata.Add((double)purchaseTotal);
            }
            var p = Fit.Line(xdata.ToArray(), ydata.ToArray());
            var intercept = p.Item1;
            var slope = p.Item2;
            foreach (var point in xdata)
            {
                var regression = slope * point + intercept;
                var timespan = TimeSpan.FromHours(point);
                totalPurchaseRegressionTraceData.X.Add(oldestPurchaseDate + timespan);
                totalPurchaseRegressionTraceData.Y.Add(regression);
            }
            chart.Data.Add(totalPurchaseRegressionTraceData);

            // TODO: Capture purchases with null category
            var categories = purchases.Where(x => x.CategoryId != null).Select(x => x.Category).Distinct().ToList();
            foreach (var category in categories)
            {
                var categoryTraceData = new TraceData {Name = category.Name, Type = "scatter", Mode = "markers"};
                var purchasesInCategory = purchases.Where(x => x.CategoryId == category.Id).ToList();
                foreach (var purchaseInCategory in purchasesInCategory)
                {
                    categoryTraceData.X.Add(purchaseInCategory.OrderDate.Date);
                    categoryTraceData.Y.Add(purchaseInCategory.UnitPrice * purchaseInCategory.Quantity);
                }
                chart.Data.Add(categoryTraceData);
            }
            return chart;
        }

        private static Chart GetPurchasesByNecessityValueChart(ICollection<Purchase> purchases)
        {
            var chart = new Chart {Layout = {Title = "Purchases by Necessity"}};

            // TODO: Capture purchases with null category
            var necessityValues = purchases.Select(x => x.GetNecessityValue()).Distinct();
            foreach (var necessityValue in necessityValues)
            {
                var necessityValueTraceData =
                    new TraceData {Name = necessityValue.ToDescription(), Type = "scatter", Mode = "markers"};
                var xdata = new List<double>();
                var ydata = new List<double>();
                var purchasesWithNecessityValue = purchases
                    .Where(x => x.GetNecessityValue() == necessityValue).ToList();
                var oldestPurchaseDate = purchasesWithNecessityValue.OrderBy(x => x.OrderDate).First().OrderDate;
                foreach (var purchaseWithNecessityValue in purchasesWithNecessityValue)
                {
                    var purchaseTotal = purchaseWithNecessityValue.UnitPrice * purchaseWithNecessityValue.Quantity;
                    var timespan = purchaseWithNecessityValue.OrderDate - oldestPurchaseDate;
                    xdata.Add(timespan.TotalHours);
                    ydata.Add((double) purchaseTotal);
                    necessityValueTraceData.X.Add(purchaseWithNecessityValue.OrderDate.Date);
                    necessityValueTraceData.Y.Add(purchaseTotal);
                }
                chart.Data.Add(necessityValueTraceData);

                var necessityRegresstionTraceData = new TraceData
                {
                    Name = $"{necessityValue.ToDescription()} regression",
                    Type = "scatter",
                    Mode = "lines",
                    Line = {Dash = "dot", Width = 4}
                };
                var p = Fit.Line(xdata.ToArray(), ydata.ToArray());
                var intercept = p.Item1;
                var slope = p.Item2;
                foreach (var point in xdata)
                {
                    var regression = slope * point + intercept;
                    var timespan = TimeSpan.FromHours(point);
                    necessityRegresstionTraceData.X.Add(oldestPurchaseDate + timespan);
                    necessityRegresstionTraceData.Y.Add(regression);
                }
                chart.Data.Add(necessityRegresstionTraceData);
            }
            return chart;
        }
    }
}