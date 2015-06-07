using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using AutoMapper;
using Estream.Cart42.Web.Domain;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Areas.Admin.Models
{
    public class ProductSectionEditViewModel : IHaveCustomMappings
    {
        public ProductSectionEditViewModel()
        {
            Type = ProductSectionType.Text;
            Position = ProductSectionPosition.Tabs;
            Priority = 1;
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public string Settings { get; set; }

        [AllowHtml]
        public string Text { get; set; }

        public ProductSectionType Type { get; set; }

        public ProductSectionPosition Position { get; set; }

        public int Priority { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            Mapper.CreateMap<ProductSection, ProductSectionEditViewModel>()
                .ForMember(s => s.Text, opt => opt.MapFrom(s => getTextFromSettings(s)));

            Mapper.CreateMap<ProductSectionEditViewModel, ProductSection>()
                .ForMember(s => s.Settings, opt => opt.MapFrom(s => s.Settings ?? getSettingsJson(s)));
        }

        private static string getTextFromSettings(ProductSection section)
        {
            if (section.Settings == null || section.Type != ProductSectionType.Text) return null;
            var settings = JsonConvert.DeserializeObject<ProductSectionTextSettings>(section.Settings);
            if (settings == null) return null;
            return settings.Text;
        }

        private static string getSettingsJson(ProductSectionEditViewModel section)
        {
            if (section.Type == ProductSectionType.Text)
            {
                var settings = new ProductSectionTextSettings {Text = section.Text};
                return JsonConvert.SerializeObject(settings);
            }
            return null;
        }
    }
}