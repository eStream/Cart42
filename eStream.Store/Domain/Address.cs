using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estream.Cart42.Web.Domain
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public AddressType Type { get; set; }

        public bool IsPrimary { get; set; }

        [MaxLength(200)]
        public string FirstName { get; set; }

        [MaxLength(200)]
        public string LastName { get; set; }

        [MaxLength(200)]
        public string Company { get; set; }

        [MaxLength(200)]
        public string Address1 { get; set; }

        [MaxLength(200)]
        public string Address2 { get; set; }

        [MaxLength(200)]
        public string City { get; set; }

        [MaxLength(2)]
        public string CountryCode { get; set; }

        public int? RegionId { get; set; }

        [MaxLength(200)]
        public string RegionOther { get; set; }

        [MaxLength(200)]
        public string ZipPostal { get; set; }

        [MaxLength(200)]
        public string Phone { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<Order> Orders_Billing { get; set; }

        public virtual ICollection<Order> Orders_Shipping { get; set; }

        [ForeignKey("CountryCode")]
        public virtual Country Country { get; set; }

        [ForeignKey("RegionId")]
        public virtual Region Region { get; set; }
    }

    public enum AddressType
    {
        Billing = 0,
        Shipping = 1
    }
}