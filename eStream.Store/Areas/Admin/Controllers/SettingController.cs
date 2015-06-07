using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class SettingController : BaseController
    {
        private readonly ISettingService settings;

        public SettingController(DataContext db, ISettingService settings) : base(db)
        {
            this.settings = settings;
        }

        // GET: Admin/Setting
        [AccessAuthorize(OperatorRoles.SETTINGS)]
        public ActionResult Index()
        {
            var model = new List<SettingsViewModel>();

            foreach (object enumVal in Enum.GetValues(typeof (SettingField)))
            {
                if (((SettingField) enumVal).GetAttributeOfType<CategoryAttribute>() == null)
                    continue;

                var setting = new SettingViewModel();
                setting.Key = Enum.GetName(typeof (SettingField), enumVal);
                setting.Name = ((SettingField) enumVal).GetAttributeOfType<NameAttribute>().Name.TA();

                setting.Value = settings.Get<object>(((SettingField) enumVal)).ToString();

                setting.EditorType = ((SettingField) enumVal).GetAttributeOfType<UIHintAttribute>() != null
                    ? ((SettingField) enumVal).GetAttributeOfType<UIHintAttribute>().UIHint
                    : ((SettingField) enumVal).GetAttributeOfType<TypeAttribute>().Type.Name;

                var category = ((SettingField) enumVal).GetAttributeOfType<CategoryAttribute>().Category.TA();

                if (model.None(s => s.Category == category))
                    model.Add(new SettingsViewModel { Category = category, Settings = new List<SettingViewModel>() });
                model.Single(s => s.Category == category).Settings.Add(setting);
            }

            return View(model);
        }

        // POST: Admin/Setting
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessAuthorize(OperatorRoles.SETTINGS + OperatorRoles.WRITE)]
        public ActionResult Index(List<SettingsViewModel> model)
        {
            if (ModelState.IsValid)
            {
                foreach (var category in model)
                {
                    foreach (SettingViewModel setting in category.Settings)
                    {
                        var field = (SettingField) Enum.Parse(typeof (SettingField), setting.Key);
                        if (field.GetAttributeOfType<TypeAttribute>().Type == typeof (bool))
                        {
                            settings.Set(field, !string.IsNullOrEmpty(setting.Value));
                        }
                        else
                        {
                            settings.Set(field, setting.Value);
                        }
                    }
                }
            }

            return RedirectToAction("Index")
                .WithSuccess("The site settings have been updated".TA());
        }
    }
}