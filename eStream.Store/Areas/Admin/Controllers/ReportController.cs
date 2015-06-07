using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ReportController : BaseController
    {
        public const int CACHE_DURATION = 30 * 60 * 60; // 30 minutes

        public ReportController(DataContext db)
            : base(db)
        {
        }

        [OutputCache(Duration = CACHE_DURATION, Location = OutputCacheLocation.Server, VaryByCustom = "lang")]
        [AccessAuthorize(OperatorRoles.REPORTS)]
        public async Task<PartialViewResult> SalesBox()
        {
            var model = new SalesBoxViewModel();

            if (db.Orders.None())
            {
                model.Amount = 12580.50m;
                model.Difference = 98;
            }
            else
            {
                var fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var toDate = DateTime.Now;

                model.Amount = await (from o in db.Orders
                    where o.DatePlaced >= fromDate && o.DatePlaced <= toDate
                    select o.Total).DefaultIfEmpty(0).SumAsync();

                toDate = fromDate;
                fromDate = fromDate.AddMonths(-1);

                var previousAmount = await (from o in db.Orders
                    where o.DatePlaced >= fromDate && o.DatePlaced < toDate
                    select o.Total).DefaultIfEmpty(0).SumAsync();

                if (previousAmount > 0)
                    model.Difference = (int)Math.Round((model.Amount / previousAmount) * 100m);
            }

            return PartialView("_SalesBox", model);
        }

        [OutputCache(Duration = CACHE_DURATION, Location = OutputCacheLocation.Server, VaryByCustom = "lang")]
        [AccessAuthorize(OperatorRoles.REPORTS)]
        public async Task<PartialViewResult> OrdersBox()
        {
            var model = new OrdersBoxViewModel();

            if (db.Orders.None())
            {
                model.Number = 251;
                model.Difference = 20;
            }
            else
            {
                var fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var toDate = DateTime.Now;

                model.Number = await db.Orders.CountAsync(o => o.DatePlaced >= fromDate && o.DatePlaced <= toDate);

                toDate = fromDate;
                fromDate = fromDate.AddMonths(-1);
                var lastMonthDays = DateTime.DaysInMonth(fromDate.Year, fromDate.Month);

                var previousAmount = await db.Orders.CountAsync(o => o.DatePlaced >= fromDate && o.DatePlaced < toDate);

                if (previousAmount == 0)
                {
                    model.Difference = 0;
                }
                else
                {
                    model.Difference = (int)Math.Round(((model.Number / (decimal)DateTime.Now.Day) /
                                        (previousAmount / (decimal)lastMonthDays)) * 100m);
                }
            }

            return PartialView("_OrdersBox", model);
        }

        [OutputCache(Duration = CACHE_DURATION, Location = OutputCacheLocation.Server, VaryByCustom = "lang")]
        [AccessAuthorize(OperatorRoles.REPORTS)]
        public async Task<PartialViewResult> VisitsBox()
        {
            var model = new VisitsBoxViewModel();

            if (db.Orders.None())
            {
                model.Number = 16120;
                model.Difference = 44;
            }
            else
            {
                var today = DateTime.Now.Date;
                model.Number = await db.Visitors.CountAsync(v => v.LastVisitDate >= today);

                var todayProrated = (model.Number/DateTime.Now.TimeOfDay.TotalMinutes)*(24*60);

                var yesterday = DateTime.Now.AddDays(-1).Date;
                var yesterdayCount = await db.Visitors.CountAsync(v =>
                    DbFunctions.TruncateTime(v.LastVisitDate) == yesterday);

                if (yesterdayCount == 0)
                    model.Difference = 100;
                else
                    model.Difference = (int)((todayProrated / yesterdayCount) * 100);
            }

            return PartialView("_VisitsBox", model);
        }

        [OutputCache(Duration = CACHE_DURATION, Location = OutputCacheLocation.Server, VaryByCustom = "lang")]
        [AccessAuthorize(OperatorRoles.REPORTS)]
        public async Task<PartialViewResult> ReturnsBox()
        {
            var model = new ReturnsBoxViewModel();

            if (db.Orders.None())
            {
                model.Rate = 2.8m;
                model.Difference = -12;
            }
            else
            {
                var totalOrders = db.Orders.Count();
                var returnedOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.Returned);
                model.Rate = ((decimal) returnedOrders/(decimal) totalOrders)*100m;
                model.Difference = 0;
            }

            return PartialView("_ReturnsBox", model);
        }

        [OutputCache(Duration = CACHE_DURATION, Location = OutputCacheLocation.Server, VaryByCustom = "lang")]
        [AccessAuthorize(OperatorRoles.REPORTS)]
        public async Task<PartialViewResult> OrdersChartBox()
        {
            var model = new OrdersChartBoxViewModel();

            if (db.Orders.None())
            {
                var rand = new Random();

                var fromDate = DateTime.Now.Date.AddDays(-30);
                var toDate = DateTime.Now.Date;

                foreach (var date in fromDate.EachDay(toDate))
                {
                    model.OrdersData.Add(new DataItemViewModel { Date = date, Value = rand.Next(50, 120) });
                    model.SalesData.Add(new DataItemViewModel { Date = date, Value = rand.Next(2000, 6000) });
                }
            }
            else
            {
                var fromDate = DateTime.Now.Date.AddDays(-30);
                var toDate = DateTime.Now;

                var data = await (from o in db.Orders
                    where o.DatePlaced >= fromDate && o.DatePlaced <= toDate
                    group o.Total by DbFunctions.TruncateTime(o.DatePlaced)
                    into g
                    select new
                           {
                               Date = g.Key,
                               Orders = g.Count(),
                               Sales = g.Sum()
                           }).ToListAsync();

                foreach (var date in fromDate.EachDay(toDate))
                {
                    model.OrdersData.Add(new DataItemViewModel { Date = date, Value = data.Where(d => d.Date == date).Select(d => d.Orders).SingleOrDefault() });
                    model.SalesData.Add(new DataItemViewModel { Date = date, Value = data.Where(d => d.Date == date).Select(d => d.Sales).SingleOrDefault() });
                }
            }

            return PartialView("_OrdersChartBox", model);
        }

        [OutputCache(Duration = CACHE_DURATION, Location = OutputCacheLocation.Server, VaryByCustom = "lang")]
        [AccessAuthorize(OperatorRoles.REPORTS)]
        public async Task<PartialViewResult> TopCategoriesBox()
        {
            var model = new TopCategoriesBoxViewModel();

            if (db.Orders.None())
            {
                model.CategoriesData = new List<CategoryDataViewModel>()
                                       {
                                           new CategoryDataViewModel { Name = "Shoes", Orders = 72, Total = 1223.9m},
                                           new CategoryDataViewModel { Name = "Clothes", Orders = 30, Total = 275.5m},
                                           new CategoryDataViewModel { Name = "Accessories", Orders = 72, Total = 122.0m},
                                       };
            }
            else
            {
                var data = from oi in db.OrderItems
                    from c in db.Categories
                    where oi.Product.Categories.Contains(c)
                    select new
                           {
                               CategoryId = c.Id,
                               Name = c.Name,
                               Amount = oi.Quantity * oi.ItemPrice,
                           };

                var data2 = from d in data
                    group d by new {d.CategoryId, d.Name}
                    into g
                    orderby g.Count() descending 
                    select new CategoryDataViewModel
                           {
                               Name = g.Key.Name,
                               Orders = g.Count(),
                               Total = g.Sum(s => s.Amount)
                           };

                model.CategoriesData = await data2.Take(10).ToListAsync();
            }

            return PartialView("_TopCategoriesBox", model);
        }

        [OutputCache(Duration = CACHE_DURATION, Location = OutputCacheLocation.Server, VaryByCustom = "lang")]
        [AccessAuthorize(OperatorRoles.REPORTS)]
        public async Task<PartialViewResult> OrdersMapBox()
        {
            var model = new OrdersMapBoxViewModel();

            if (db.Orders.None())
            {
                model.OrdersData = new List<MapDataViewModel>
                                   {
                                       new MapDataViewModel { Code = "BG", Value = 120 }, 
                                       new MapDataViewModel { Code = "DE", Value = 1120 }, 
                                       new MapDataViewModel { Code = "US", Value = 5120 }, 
                                       new MapDataViewModel { Code = "UK", Value = 220}, 
                                       new MapDataViewModel { Code = "FR", Value = 110 }, 
                                   };
            }
            else
            {
                var data = await (from o in db.Orders
                    group o by o.BillingAddress.CountryCode
                    into g
                    select new MapDataViewModel
                           {
                               Code = g.Key,
                               Value = g.Count()
                           }).ToListAsync();

                model.OrdersData = data;
            }

            return PartialView("_OrdersMapBox", model);
        }
    }
}