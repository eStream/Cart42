using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estream.Cart42.Web.Domain
{
    public class TaxClassRate
    {
        [Key, Column(Order = 0)]
        public int TaxRateId { get; set; }

        [Key, Column(Order = 1)]
        public int TaxClassId { get; set; }

        public decimal Amount { get; set; }

        [ForeignKey("TaxClassId")]
        public virtual TaxClass TaxClass { get; set; }

        [ForeignKey("TaxRateId")]
        public virtual TaxRate TaxRate { get; set; }
    }
}