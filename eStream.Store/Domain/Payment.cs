using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estream.Cart42.Web.Domain
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int OrderId { get; set; }

        public int PaymentMethodId { get; set; }

        public PaymentStatus Status { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public string Notes { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod PaymentMethod { get; set; }
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        ManualReview = 4,
        Refunded = 5
    }
}