using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Domain
{
    public class Return
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int OrderId { get; set; }

        public ReturnStatus Status { get; set; }

        public ReturnReason Reason { get; set; }

        public ReturnAction Action { get; set; }

        public string UserComments { get; set; }

        public string StaffNotes { get; set; }

        public bool CreditIssued { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        public virtual ICollection<ReturnItem> Items { get; set; }
    }

    public enum ReturnReason
    {
        WrongReceived = 1,
        WrongOrdered = 2,
        NotSatisfied = 3,
        Problem = 4
    }

    public enum ReturnAction
    {
        Repair = 1,
        Replacement = 2,
        StoreCredit = 3,
    }

    public enum ReturnStatus
    {
        Pending = 1,
        Received = 2,
        Authorized = 3,
        Repaired = 4,
        Refunded = 5,
        Rejected = 6,
        Cancelled = 7
    }
}