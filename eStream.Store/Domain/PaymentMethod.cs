using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Estream.Cart42.Web.Domain
{
    public class PaymentMethod
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Display Name")]
        public string Name { get; set; }

        public string ClassName { get; set; }

        // Json
        public string Settings { get; set; }

        public PaymentMethodType Type { get; set; }

        public bool IsActive { get; set; }

        // If empty then all countries
        public virtual ICollection<Country> Countries { get; set; }
    }

    public enum PaymentMethodType
    {
        [Display(Name = "Manual")]
        Manual = 1,
        [Display(Name = "Hosted")]
        Hosted = 2
    }
}