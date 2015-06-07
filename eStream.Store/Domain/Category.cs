using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using CsvHelper.Configuration;
using Estream.Cart42.Web.DAL;

namespace Estream.Cart42.Web.Domain
{
    public class Category
    {
        public Category()
        {
            Products = new Collection<Product>();
            ChildCategories = new Collection<Category>();
            IsVisible = true;
        }

        [Key]
        public int Id { get; set; }

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Category Parent { get; set; }

        public string Name { get; set; }

        public string NameWithParent
        {
            get
            {
                string name = Name;
                if (ParentId != null)
                {
                    int? parentId = ParentId;
                    do
                    {
                        Category parent = DataContext.Current.Categories.First(c => c.Id == parentId);
                        name = parent.Name + " > " + name;
                        parentId = parent.ParentId;
                    } while (parentId != null);
                }

                return name;
            }
        }

        public string Description { get; set; }

        public bool IsVisible { get; set; }

        public int SortOrder { get; set; }

        public virtual ICollection<Product> Products { get; set; }

        public virtual ICollection<Category> ChildCategories { get; set; }
    }

    public sealed class CategoryCsvMap : CsvClassMap<Category>
    {
        public CategoryCsvMap()
        {
            Map(m => m.Id);
            Map(m => m.ParentId);
            Map(m => m.Name);
            Map(m => m.Description);
            Map(m => m.IsVisible);
            Map(m => m.SortOrder);
        }
    }
}