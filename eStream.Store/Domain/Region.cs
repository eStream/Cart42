using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estream.Cart42.Web.Domain
{
    public class Region
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Code { get; set; }

        [MaxLength(10)]
        public string CountryCode { get; set; }

        [Required, MaxLength(500)]
        public string Name { get; set; }

        [ForeignKey("CountryCode")]
        public virtual Country Country { get; set; }

        public virtual ICollection<TaxZone> TaxZones { get; set; }

        public virtual ICollection<ShippingZone> ShippingZones { get; set; } 
    }
}