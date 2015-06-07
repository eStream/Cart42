using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class SalesBoxViewModel
    {
        public decimal Amount { get; set; }
        public int? Difference { get; set; }
    }

    public class OrdersBoxViewModel
    {
        public int Number { get; set; }
        public int Difference { get; set; }
    }

    public class VisitsBoxViewModel
    {
        public int Number { get; set; }
        public int Difference { get; set; }
    }

    public class ReturnsBoxViewModel
    {
        public decimal Rate { get; set; }
        public int Difference { get; set; }
    }

    public class OrdersChartBoxViewModel
    {
        public OrdersChartBoxViewModel()
        {
            OrdersData = new List<DataItemViewModel>();
            SalesData = new List<DataItemViewModel>();
        }

        public List<DataItemViewModel> OrdersData;
        public List<DataItemViewModel> SalesData;
    }

    public class DataItemViewModel
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }

    public class OrdersMapBoxViewModel
    {
        public OrdersMapBoxViewModel()
        {
            OrdersData = new List<MapDataViewModel>();
        }

        public List<MapDataViewModel> OrdersData;
    }

    public class MapDataViewModel
    {
        public string Code { get; set; }
        public int Value { get; set; }
    }

    public class TopCategoriesBoxViewModel
    {
        public TopCategoriesBoxViewModel()
        {
            CategoriesData = new List<CategoryDataViewModel>();
        }

        public List<CategoryDataViewModel> CategoriesData;
    }

    public class CategoryDataViewModel
    {
        public string Name { get; set; }
        public int Orders { get; set; }
        public decimal Total { get; set; }
    }
}