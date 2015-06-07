using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Estream.Cart42.Web.Services;
using FluentValidation;

namespace Estream.Cart42.Web.Domain
{
    public class TaxClass
    {
        [Key]
        public int Id { get; set; }
        
        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}