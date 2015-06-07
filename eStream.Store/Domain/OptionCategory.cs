using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using FluentValidation;

namespace Estream.Cart42.Web.Domain
{
    [DisplayName("Options category")]
    public class OptionCategory
    {
        public OptionCategory()
        {
            Options = new Collection<Option>();
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }


        public string Description { get; set; }

        public OptionCategoryType Type { get; set; }

        [Display(Name = "Include in filters")]
        public bool IncludeInFilters { get; set; }

        [IgnoreMap]
        public virtual ICollection<Option> Options { get; set; }
    }

    public enum OptionCategoryType
    {
        [Display(Name = "Dropdown")]
        Dropdown = 1,
        [Display(Name = "Checkbox")]
        Checkbox = 2,
        [Display(Name = "Radiobutton")]
        Radiobutton = 3,
        [Display(Name = "Text")]
        Text = 4,
        [Display(Name = "Boxes")]
        Boxes = 5,
        [Display(Name = "Colors")]
        Colors = 6
    }
}