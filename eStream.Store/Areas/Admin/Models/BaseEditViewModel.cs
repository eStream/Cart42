using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class BaseEditViewModel
    {
        public OnCompleteActionType OnCompleteAction { get; set; }
    }

    public enum OnCompleteActionType
    {
        [Display(Name = "Save")]
        Nothing,
        [Display(Name = "Save and add another")]
        AddNew,
        [Display(Name = "Save and copy to another")]
        CloneNew
    }
}