using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;

namespace Estream.Cart42.Web.Domain
{
    public class TaxRate
    {
        [Key]
        public int Id { get; set; }

        public int TaxZoneId { get; set; }

        public string Name { get; set; }

        public decimal Amount { get; set; }

        public bool IsActive { get; set; }

        public int Order { get; set; }

        [ForeignKey("TaxZoneId")]
        public virtual TaxZone TaxZone { get; set; }

        [IgnoreMap]
        public virtual ICollection<TaxClassRate> ClassRates { get; set; }
    }
}