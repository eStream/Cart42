using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Estream.Cart42.Web.Domain
{
    public class ShippingZone
    {
        public ShippingZone()
        {
            Countries = new Collection<Country>();
            Regions = new Collection<Region>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Country> Countries { get; set; }

        public virtual ICollection<Region> Regions { get; set; }

        public bool IsActive { get; set; }
    }
}