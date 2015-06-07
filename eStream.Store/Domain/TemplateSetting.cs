using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Estream.Cart42.Web.Domain
{
    public class TemplateSetting
    {
        [Key]
        public int Id { get; set; }

        public string TemplateName { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}