using System.ComponentModel.DataAnnotations;

namespace Estream.Cart42.Web.Domain
{
    public class Setting
    {
        [Key]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}