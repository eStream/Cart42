using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Models
{
    public class SearchViewModel
    {
        public Category[] Categories { get; set; }

        public int? CategoryId { get; set; }

        public string Keywords { get; set; }
    }

    public class SearchResultsViewModel
    {
        public int? CategoryId { get; set; }

        public string Keywords { get; set; }
    }
}