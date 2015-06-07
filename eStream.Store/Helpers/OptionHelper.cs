using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Models;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Helpers
{
    public static class OptionHelper
    {
        public static string[] GetOptionNames(string optionsJson)
        {
            if (optionsJson == null) return null;
            var optionsDeserialized = JsonConvert.DeserializeObject<ShoppingCartItemOptionViewModel[]>(optionsJson);
            int[] optionIds = optionsDeserialized.Select(o => o.id).ToArray();
            List<Option> options =
                DataContext.Current.Options.Where(o => optionIds.Contains(o.Id)).Include(o => o.Category).ToList();

            return options.Select(o => o.Category).Distinct().Select(c => string.Format("{0}: {1}",
                c.Name, string.Join(", ", options.Where(o => o.OptionCategoryId == c.Id).Select(o => o.Name))))
                .ToArray();
        }
    }
}