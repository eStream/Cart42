using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Models
{
    public interface ISortableViewModel
    {
        string OrderColumn { get; set; }
        bool OrderAsc { get; set; }
    }
}