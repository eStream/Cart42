using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;
using Ionic.Zip;
using Newtonsoft.Json;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class PageTemplateController : BaseController
    {
        private readonly ISettingService settingService;
        private readonly ITemplateSettingService templateSettingService;

        public PageTemplateController(DataContext db, ISettingService settingService, 
            ITemplateSettingService templateSettingService) : base(db)
        {
            this.settingService = settingService;
            this.templateSettingService = templateSettingService;
        }

        // GET: Admin/PageTemplate
        [AccessAuthorize(OperatorRoles.TEMPLATES)]
        public ActionResult Index()
        {
            var model = new PageTemplatesIndexViewModel();
            model.SelectedTemplate = settingService.Get<string>(SettingField.Theme);

            var viewDirectory = Server.MapPath("~/Views");
            foreach (var directory in Directory.GetDirectories(viewDirectory))
            {
                var name = directory.Substring(directory.LastIndexOf('\\') + 1);
                if (name == "Shared") continue;

                var thumbUrl = Url.Content("~/Content/Themes/" + name + "/screenshot.png");

                if (!System.IO.File.Exists(Server.MapPath(thumbUrl)))
                {
                    try
                    {
                        ScreenshotHelper.GenerateScreenshot(
                            Url.Action("Index", "Home", new { area = "" }, Request.Url.Scheme) + "?theme=" + name,
                            Server.MapPath(thumbUrl));
                    }
                    catch
                    {
                        // Unable to generate screenshot. No big deal
                    }
                }

                thumbUrl += "?seed=" + new Random().Next();

                model.Templates.Add(new PageTemplateIndexViewModel
                                    {
                                        Name = name,
                                        Thumbnail = thumbUrl
                                    });
            }

            return View(model);
        }

        [AccessAuthorize(OperatorRoles.TEMPLATES + OperatorRoles.WRITE)]
        public ActionResult Clone(string name, string newName)
        {
            if (name.Contains(".") || name.Contains("/") || name.Contains("\\")
                || newName.Contains(".") || newName.Contains("/") || newName.Contains("\\"))
                return RedirectToAction("Index").WithError("Invalid template name".TA());

            var viewDirectory = Server.MapPath("~/Views");

            var sourceDir = Path.Combine(viewDirectory, name);
            if (!Directory.Exists(sourceDir))
                return RedirectToAction("Index").WithError(string.Format("Template {0} doesn't exist".TA(), name));

            var targetDir = Path.Combine(viewDirectory, newName);
            if (Directory.Exists(targetDir))
                return RedirectToAction("Index").WithError(string.Format("Template {0} already exists".TA(), newName));

            FileHelper.CopyDirectory(sourceDir, targetDir);

            var contentDirectory = Server.MapPath("~/Content/Themes");
            sourceDir = Path.Combine(contentDirectory, name);
            targetDir = Path.Combine(contentDirectory, newName);

            FileHelper.CopyDirectory(sourceDir, targetDir);

            refreshBundles();

            return RedirectToAction("Index").WithSuccess("Template has been cloned successfully".TA());
        }

        [AccessAuthorize(OperatorRoles.TEMPLATES)]
        public FilePathResult Download(string name)
        {
            if (name.Contains(".") || name.Contains("/") || name.Contains("\\"))
                return null;

            var exportDir = Server.MapPath("~/Export");
            var zipFile = Path.Combine(exportDir,
                "Template_" + name + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".zip");

            var viewsDir = Path.Combine(Server.MapPath("~/Views"), name);
            var contentDir = Path.Combine(Server.MapPath("~/Content/Themes"), name);

            using (var zip = new ZipFile(zipFile))
            {
                zip.AddDirectory(viewsDir, "Views");
                zip.AddDirectory(contentDir, "Content");

                zip.Save();
            }

            var result = new FilePathResult(zipFile, "application/zip");
            result.FileDownloadName = "Template_" + name + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".zip";
            return result;
        }

        [AccessAuthorize(OperatorRoles.TEMPLATES + OperatorRoles.WRITE)]
        public ActionResult Select(string name)
        {
            if (name.Contains(".") || name.Contains("/") || name.Contains("\\"))
                return null;

            var viewDirectory = Server.MapPath("~/Views");

            var sourceDir = Path.Combine(viewDirectory, name);
            if (!Directory.Exists(sourceDir))
                return RedirectToAction("Index").WithError(string.Format("Template {0} doesn't exist".TA(), name));

            settingService.Set(SettingField.Theme, name);

            return
                RedirectToAction("Index")
                    .WithSuccess(string.Format("Template {0} has been selected as the active site template".TA(), name));
        }

        [AccessAuthorize(OperatorRoles.TEMPLATES + OperatorRoles.WRITE)]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [AccessAuthorize(OperatorRoles.TEMPLATES + OperatorRoles.WRITE)]
        public ActionResult Create(string name, HttpPostedFileBase file)
        {
            if (name.Contains(".") || name.Contains("/") || name.Contains("\\")
                || file == null || file.ContentLength <= 0)
                return null;

            var importDir = Server.MapPath("~/Import");
            var zipFile = Path.Combine(importDir,
                "Template_" + name + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".zip");

            file.SaveAs(zipFile);

            var viewsDir = Path.Combine(Server.MapPath("~/Views"), name);
            if (!Directory.Exists(viewsDir))
                Directory.CreateDirectory(viewsDir);

            var contentDir = Path.Combine(Server.MapPath("~/Content/Themes"), name);
            if (!Directory.Exists(contentDir))
                Directory.CreateDirectory(contentDir);

            using (var zip = ZipFile.Read(zipFile))
            {
                foreach (var zipEntry in zip.Entries)
                {
                    string fileName;
                    string targetPath;
                    if (zipEntry.FileName.StartsWith("Views/"))
                    {
                        fileName = zipEntry.FileName.Replace("Views/", "");
                        targetPath = Path.Combine(viewsDir, fileName);
                    }
                    else if (zipEntry.FileName.StartsWith("Content/"))
                    {
                        fileName = zipEntry.FileName.Replace("Content/", "");
                        targetPath = Path.Combine(contentDir, fileName);
                    }
                    else
                    {
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(fileName)) continue;
                    if (zipEntry.IsDirectory)
                    {
                        if (!Directory.Exists(targetPath))
                            Directory.CreateDirectory(targetPath);
                    }
                    else
                    {
                        using (var outfile = System.IO.File.OpenWrite(targetPath))
                        {
                            zipEntry.Extract(outfile);
                        }
                    }
                }
            }

            try
            {
                var thumbUrl = Url.Content("~/Content/Themes/" + name + "/screenshot.png");
                ScreenshotHelper.GenerateScreenshot(
                    Url.Action("Index", "Home", new { area = "" }, Request.Url.Scheme) + "?theme=" + name,
                    Server.MapPath(thumbUrl));
            }
            catch
            {
                // Unable to generate screenshot. No big deal
            }

            refreshBundles();

            return RedirectToAction("Index").WithSuccess("The template has been uploaded successfully".TA());
        }

        [AccessAuthorize(OperatorRoles.TEMPLATES)]
        public ActionResult Files(string name)
        {
            if (name.Contains(".") || name.Contains("/") || name.Contains("\\"))
                return null;

            var model = new PageTemplateEditViewModel { Name = name };

            var cssDirectory = Path.Combine(Server.MapPath("~/Content/Themes"), name, "css");
            foreach (var file in Directory.GetFiles(cssDirectory, "*", SearchOption.AllDirectories))
            {
                var fileName = file.Replace(cssDirectory, "").Trim('\\');
                model.CssFiles.Add(new CssFileViewModel
                {
                    Name = fileName.Replace('\\', '|'),
                    LastUpdated = System.IO.File.GetLastWriteTime(file)
                });
            }

            var viewDirectory = Path.Combine(Server.MapPath("~/Views"), name);
            foreach (var file in Directory.GetFiles(viewDirectory, "*", SearchOption.AllDirectories))
            {
                var fileName = file.Replace(viewDirectory, "").Trim('\\');
                if (fileName.IndexOf('\\') < 0) continue;
                var ctrl = fileName.Split('\\')[0];
                var act = fileName.Split('\\').Last();
                model.ViewFiles.Add(new ViewFileViewModel
                                    {
                                        Name = fileName.Replace('\\', '|'),
                                        Controller = ctrl,
                                        Action = act.Remove(act.LastIndexOf('.')),
                                        LastUpdated = System.IO.File.GetLastWriteTime(file)
                                    });
            }

            return View(model);
        }

        [AccessAuthorize(OperatorRoles.TEMPLATES)]
        public ContentResult LoadFile(string theme, string name)
        {
            if (theme.Contains(".") || theme.Contains("/") || theme.Contains("\\")
                || name.Contains("..") || name.Contains("/") || name.Contains("\\"))
                return null;

            name = name.Replace('|', '\\');

            string filePath;
            if (name.ToLowerInvariant().EndsWith(".cshtml"))
                filePath = Path.Combine(Server.MapPath("~/Views"), theme, name);
            else if (name.ToLowerInvariant().EndsWith(".css"))
                filePath = Path.Combine(Server.MapPath("~/Content/Themes"), theme, "css", name);
            else return
                    null;

            var result = new ContentResult();
            result.Content = System.IO.File.ReadAllText(filePath);

            return result;
        }

        [ValidateInput(false)]
        [AccessAuthorize(OperatorRoles.TEMPLATES + OperatorRoles.WRITE)]
        public StandardJsonResult SaveFile(string theme, string name, string contents)
        {
            if (theme.Contains(".") || theme.Contains("/") || theme.Contains("\\")
                || name.Contains("..") || name.Contains("/") || name.Contains("\\"))
                return null;

            name = name.Replace('|', '\\');

            string filePath;
            if (name.ToLowerInvariant().EndsWith(".cshtml"))
                filePath = Path.Combine(Server.MapPath("~/Views"), theme, name);
            else if (name.ToLowerInvariant().EndsWith(".css"))
                filePath = Path.Combine(Server.MapPath("~/Content/Themes"), theme, "css", name);
            else return
                    null;

            System.IO.File.WriteAllText(filePath, contents);

            return JsonSuccess<string>(null);
        }

        [AccessAuthorize(OperatorRoles.TEMPLATES)]
        public FilePathResult DownloadFile(string theme, string name)
        {
            if (theme.Contains(".") || theme.Contains("/") || theme.Contains("\\")
                || name.Contains("..") || name.Contains("/") || name.Contains("\\"))
                return null;

            name = name.Replace('|', '\\');

            var filePath = Path.Combine(Server.MapPath("~/Views"), theme, name);

            var result = new FilePathResult(filePath, "application/octet-stream");
            result.FileDownloadName = Path.GetFileName(filePath);
            return result;
        }

        [HttpPost]
        [AccessAuthorize(OperatorRoles.TEMPLATES + OperatorRoles.WRITE)]
        public ActionResult UploadFile(string theme, string name, HttpPostedFileBase file)
        {
            if (theme.Contains(".") || theme.Contains("/") || theme.Contains("\\")
                || name.Contains("..") || name.Contains("/") || name.Contains("\\"))
                return null;

            name = name.Replace('|', '\\');

            var filePath = Path.Combine(Server.MapPath("~/Views"), theme, name);

            if (file == null || file.ContentLength <= 0)
                return null;

            file.SaveAs(filePath);

            return RedirectToAction("Files", new { name = theme }).WithSuccess("The file has been uploaded successfully".TA());
        }

        [AccessAuthorize(OperatorRoles.TEMPLATES + OperatorRoles.DELETE)]
        public ActionResult Delete(string theme)
        {
            if (string.IsNullOrWhiteSpace(theme) || theme.Contains(".") || theme.Contains("/") || theme.Contains("\\")
                || theme == "Shared")
                return null;

            var filePath = Path.Combine(Server.MapPath("~/Views"), theme);
            if (Directory.Exists(filePath))
                Directory.Delete(filePath, true);

            filePath = Path.Combine(Server.MapPath("~/Content/Themes"), theme);
            if (Directory.Exists(filePath))
                Directory.Delete(filePath, true);

            return RedirectToAction("Index").WithWarning("Template has been deleted successfully".TA());
        }

        private static void refreshBundles()
        {
            BundleConfig.RegisterBundles(BundleConfig._bundles);
        }

        [AccessAuthorize(OperatorRoles.TEMPLATES)]
        public ActionResult Settings(string name)
        {
            if (name.Contains(".") || name.Contains("/") || name.Contains("\\"))
                return null;

            var viewDirectory = Path.Combine(Server.MapPath("~/Views"), name);
            if (!System.IO.File.Exists(Path.Combine(viewDirectory, "settings.json")))
                return RedirectToAction("Index").WithWarning("This template doesn't have any settings".TA());
            var settingsJson = System.IO.File.ReadAllText(Path.Combine(viewDirectory, "settings.json"));

            var model = JsonConvert.DeserializeObject<ThemeSettingsEditViewModel>(settingsJson);
            model.Name = name;
            foreach (var section in model.Sections)
            {
                foreach (var item in section.Items)
                {
                    switch (item.Type)
                    {
                        case ThemeSettingItemType.Checkbox:
                            item.ValueBool = Convert.ToBoolean(templateSettingService.GetSetting(name, item.Key));
                            break;
                        case ThemeSettingItemType.Upload:
                            // TODO:
                            break;
                        case ThemeSettingItemType.Textbox:
                        case ThemeSettingItemType.Dropdown:
                        case ThemeSettingItemType.Color:
                        case ThemeSettingItemType.Multiline:
                        case ThemeSettingItemType.Html:
                        default:
                            item.Value = templateSettingService.GetSetting(name, item.Key);
                            break;
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [AccessAuthorize(OperatorRoles.TEMPLATES + OperatorRoles.WRITE)]
        public ActionResult Settings(ThemeSettingsEditViewModel model)
        {
            foreach (var section in model.Sections)
            {
                foreach (var item in section.Items)
                {
                    switch (item.Type)
                    {
                        case ThemeSettingItemType.Checkbox:
                            templateSettingService.SetSetting(model.Name, item.Key, item.ValueBool.ToString());
                            break;
                        case ThemeSettingItemType.Upload:
                            if (item.ValueFile != null && item.ValueFile.ContentLength > 0)
                            {
                                string root = Server.MapPath("~/Storage");
                                var upload = new Upload { Type = UploadType.TemplateImage };
                                db.Uploads.Add(upload);
                                db.SaveChanges();
                                item.ValueFile.SaveAs(Path.Combine(root, upload.Id.ToString()));
                                templateSettingService.SetSetting(model.Name, item.Key, upload.Id.ToString());
                            }
                            break;
                        case ThemeSettingItemType.Textbox:
                        case ThemeSettingItemType.Dropdown:
                        case ThemeSettingItemType.Color:
                        case ThemeSettingItemType.Multiline:
                        case ThemeSettingItemType.Html:
                        default:
                            templateSettingService.SetSetting(model.Name, item.Key, item.Value);
                            break;
                    }
                }
            }

            return RedirectToAction("Settings", new { name = model.Name })
                .WithSuccess("Template settings have been updated".TA());
        }

        [AccessAuthorize(OperatorRoles.TEMPLATES + OperatorRoles.WRITE)]
        public ActionResult ResetSettings(string name)
        {
            templateSettingService.ResetSettings(name);
            return RedirectToAction("Settings", new { name = name })
                .WithSuccess("Settings have been reset".TA());
        }
    }
}