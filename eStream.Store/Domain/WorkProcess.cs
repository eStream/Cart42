using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Domain
{
    public class WorkProcess
    {
        public WorkProcess()
        {
            DateCreated = DateTime.Now;
        }

        [Key]
        public int Id { get; set; }

        public WorkProcessType Type { get; set; }

        public double PercentComplete { get; set; }

        public bool IsRunning { get; set; }

        public bool IsComplete { get; set; }

        public string Status { get; set; }

        public string Error { get; set; }

        public DateTime DateCreated { get; set; }

        public bool CancelRequested { get; set; }
    }

    public enum WorkProcessType
    {
        Export
    }
}