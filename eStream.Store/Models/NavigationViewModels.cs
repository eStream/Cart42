using System;
using System.Collections.Generic;

namespace Estream.Cart42.Web.Models
{
    public class BreadcrumbViewModel
    {
        public List<Tuple<string, string>> Nodes { get; set; }
    }
}