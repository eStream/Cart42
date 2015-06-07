using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estream.Cart42.Web.Domain
{
    public class ShippingMethod
    {
        public ShippingMethod()
        {
            WeightRanges = "[]";
            TotalRanges = "[]";
        }

        [Key]
        public int Id { get; set; }

        public int ShippingZoneId { get; set; }

        public string Name { get; set; }

        public ShippingMethodType Type { get; set; }

        public decimal? FreeShippingMinTotal { get; set; }

        public bool FreeShippingExcludeFixedShipping { get; set; }

        public decimal? FlatRateAmount { get; set; }

        public bool FlatRatePerItem { get; set; }

        // JSON of ShippingByRange[]
        public string WeightRanges { get; set; }

        // JSON of ShippingByRange[]
        public string TotalRanges { get; set; }

        [ForeignKey("ShippingZoneId")]
        public virtual ShippingZone ShippingZone { get; set; }
    }

    public enum ShippingMethodType
    {
        [Display(Name = "Free Shipping")]
        Free = 1,
        [Display(Name = "Flat Rate Shipping")]
        Flat = 2,
        [Display(Name = "Calculate by Weight")]
        ByWeight = 3,
        [Display(Name = "Calculate by Order Total")]
        ByTotal = 4
    }

    public class ShippingByRange
    {
        public decimal From { get; set; }
        public decimal To { get; set; }
        public decimal Amount { get; set; }
    }
}