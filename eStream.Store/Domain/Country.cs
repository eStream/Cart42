using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Estream.Cart42.Web.Domain
{
    public class Country
    {
        [Key, MaxLength(2)]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [MaxLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        public virtual ICollection<PaymentMethod> PaymentMethods { get; set; }
        public virtual ICollection<ShippingZone> ShippingZones { get; set; }
        public virtual ICollection<TaxZone> TaxZones { get; set; }
    }
}