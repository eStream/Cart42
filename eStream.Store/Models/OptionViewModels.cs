using System.Collections.Generic;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Models
{
    public class OptionsViewModel
    {
        public List<OptionCategoryViewModel> Categories { get; set; }
    }

    public class OptionCategoryViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public OptionCategoryType Type { get; set; }

        public List<OptionViewModel> Options { get; set; }
    }

    public class OptionViewModel : IMapFrom<Option>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class OptionsFilterViewModel
    {
        public OptionsFilterViewModel()
        {
            Categories = new List<OptionFilterCategoryViewModel>();
        }

        public List<OptionFilterCategoryViewModel> Categories { get; set; }
    }

    public class OptionFilterCategoryViewModel
    {
        public OptionFilterCategoryViewModel()
        {
            Options = new List<OptionFilterViewModel>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public OptionCategoryType Type { get; set; }

        public List<OptionFilterViewModel> Options { get; set; }
    }

    public class OptionFilterViewModel : IMapFrom<Option>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        
        public int Count { get; set; }

        public bool ActiveFilter { get; set; }
    }
}