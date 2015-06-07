using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using CsvHelper;
using Estream.Cart42.Web.Areas.Admin.Models;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UploadController = Estream.Cart42.Web.Controllers.Api.UploadController;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class DataImportController : BaseController
    {
        private readonly ICategoryService categoryService;
        private readonly IProductFinder productFinder;
        private readonly IProductService productService;

        public DataImportController(DataContext db, ICategoryService categoryService,
            IProductFinder productFinder, IProductService productService)
            : base(db)
        {
            this.categoryService = categoryService;
            this.productFinder = productFinder;
            this.productService = productService;
        }

        // GET: Admin/DataImport
        [AccessAuthorize(OperatorRoles.DATABASE + OperatorRoles.WRITE)]
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/DataImport/IT4Profit
        [AccessAuthorize(OperatorRoles.DATABASE + OperatorRoles.WRITE)]
        public ActionResult IT4Profit()
        {
            return View();
        }

        // POST: Admin/DataImport/IT4Profit
        [HttpPost]
        [AccessAuthorize(OperatorRoles.DATABASE + OperatorRoles.WRITE)]
        public ActionResult IT4Profit(HttpPostedFileBase file)
        {
            bool skipProductsWithoutPhotos = true;

            object model = "";
            if (file.ContentLength <= 0) return View();
            var xml = XDocument.Load(file.InputStream);
            if (xml.Root.Element("PRICES") != null)
            {
                foreach (var pNode in xml.Root.Element("PRICES").Elements("PRICE") /*.Take(1000)*/)
                {
                    try
                    {
                        var sku = pNode.Element("WIC").Value;

                        // Check if product exists
                        if (db.Products.None(p => p.Sku == sku)) continue;

                        var prod = db.Products.First(p => p.Sku == sku);

                        var convertRate = 1.57256m;

                        if (pNode.Element("RETAIL_PRICE") != null && pNode.Element("RETAIL_PRICE").Value != null)
                            prod.RetailPrice = decimal.Round(
                                Convert.ToDecimal(pNode.Element("RETAIL_PRICE").Value, CultureInfo.InvariantCulture) *
                                convertRate, 2);

                        if (pNode.Element("MY_PRICE") != null)
                            prod.Price = decimal.Round(
                                Convert.ToDecimal(pNode.Element("MY_PRICE").Value, CultureInfo.InvariantCulture) *
                                convertRate, 2);

                        prod.IsVisible = prod.Price != 0;

                        db.SaveChanges();

                        model += string.Format("Set price for {0}\n", sku);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                foreach (var pNode in xml.Root.Elements("Product") /*.Take(1000)*/)
                {
                    var sku = pNode.Element("ProductCode").Value;

                    // Check if product already exists
                    if (db.Products.Any(p => p.Sku == sku)) continue;

                    /*
                var imgFiles = Directory.GetFiles(@"c:\temp\images\",
                    sku.Replace('/', '_').Replace('\\', '_') + "_*.jpg");
                if (skipProductsWithoutPhotos && !imgFiles.Any()) continue;
                */

                    // Create categories if necessary
                    var parentCategoryName = pNode.Element("ProductCategory").Value;
                    var parentCategory =
                        db.Categories.FirstOrDefault(c => c.ParentId == null && c.Name == parentCategoryName);
                    if (parentCategory == null)
                    {
                        parentCategory = new Category { Name = parentCategoryName };
                        db.Categories.Add(parentCategory);
                        db.SaveChanges();
                    }

                    var categoryName = pNode.Element("ProductType").Value;
                    var category =
                        db.Categories.FirstOrDefault(c => c.ParentId == parentCategory.Id && c.Name == categoryName);
                    if (category == null)
                    {
                        category = new Category { Name = categoryName, Parent = parentCategory };
                        db.Categories.Add(category);
                        db.SaveChanges();
                    }

                    // Create product
                    var product = new Product
                                  {
                                      Sku = sku,
                                      Name = pNode.Element("ProductDescription").Value,
                                      IsVisible = true
                                  };
                    if (pNode.Element("MarketingInfo") != null)
                        product.Description = pNode.Element("MarketingInfo").Element("element").Value;
                    product.Categories.Add(category);
                    product.Categories.Add(parentCategory);

                    db.Products.Add(product);
                    db.SaveChanges();

                    // Create options
                    if (pNode.Element("AttrList") != null)
                    {
                        foreach (var oNode in pNode.Element("AttrList").Elements("element"))
                        {
                            var optionCategoryName = oNode.Attribute("Name").Value;
                            var optionCategory = db.OptionCategories.FirstOrDefault(c => c.Name == optionCategoryName);
                            if (optionCategory == null)
                            {
                                optionCategory = new OptionCategory
                                                 {
                                                     Name = optionCategoryName,
                                                     Type = OptionCategoryType.Text
                                                 };
                                db.OptionCategories.Add(optionCategory);
                            }

                            var optionName = oNode.Attribute("Value").Value;
                            var option = db.Options.FirstOrDefault(
                                o => o.OptionCategoryId == optionCategory.Id && o.Name == optionName);
                            if (option == null)
                            {
                                option = new Option { Category = optionCategory, Name = optionName };
                                db.Options.Add(option);
                            }

                            product.Options.Add(option);
                        }
                    }

                    if (pNode.Element("Images") != null)
                    {
                        var storage = HttpContext.Request.MapPath("~/Storage");
                        var wc = new WebClient();
                        foreach (var oNode in pNode.Element("Images").Elements("Image"))
                        {
                            var upload = new Upload
                                         {
                                             Type = UploadType.ProductImage,
                                             Product = product
                                         };
                            db.Uploads.Add(upload);
                            db.SaveChanges();

                            try
                            {
                                wc.DownloadFile(oNode.Value, Path.Combine(storage, upload.Id.ToString()));
                            }
                            catch
                            {
                                db.Uploads.Remove(upload);
                                db.SaveChanges();
                            }
                        }
                    }

                    // Add photos
                    /*
                try
                {
                    foreach (var imgFile in imgFiles)
                    {
                        var upload = new Upload
                                     {
                                         Type = UploadType.ProductImage,
                                         Product = product
                                     };
                        db.Uploads.Add(upload);
                        db.SaveChanges();

                        var storage = HttpContext.Request.MapPath("~/Storage");
                        System.IO.File.Copy(imgFile, Path.Combine(storage, upload.Id.ToString()));
                    }
                }
                catch (Exception err)
                {
                    model += string.Format("Error {0}\n", err.Message);
                }
                */

                    model += string.Format("Product {0}\n", sku);
                }
            }

            return View(model);
        }

        // GET: Admin/DataImport/Solytron
        [AccessAuthorize(OperatorRoles.DATABASE + OperatorRoles.WRITE)]
        public ActionResult Solytron()
        {
            return View();
        }

        [HttpPost]
        [AccessAuthorize(OperatorRoles.DATABASE + OperatorRoles.WRITE)]
        public ActionResult Solytron(string categoryGuid, string categoryName, string username, string password)
        {
            object result = string.Empty;

            var categoryXmlUrl = string.Format(
                "http://solytron.bg/products/xml/list.xml?propertyId={0}&j_u={1}&j_p={2}",
                categoryGuid, username, password);
            var categoryXml = XDocument.Load(categoryXmlUrl);

            var category = db.Categories.First(c => c.Name == categoryName);

            foreach (var pNode in categoryXml.Root.Elements("product"))
            {
                try
                {
                    var sku = pNode.Attribute("codeId").Value;

                    if (db.Products.Any(p => p.Sku == sku)) continue;

                    var groupId = pNode.Attribute("groupId").Value;

                    var productXmlUrl = string.Format(
                        "http://solytron.bg/products/xml/product.xml?codeId={0}&groupId={1}&j_u={2}&j_p={3}",
                        sku, groupId, username, password);
                    var productXml = XDocument.Load(productXmlUrl);

                    // Create product
                    var product = new Product
                    {
                        Sku = sku,
                        Name = productXml.Root.Element("name").Value,
                        CostPrice = Convert.ToDecimal(pNode.Element("price").Value, CultureInfo.InvariantCulture),
                        Price = Convert.ToDecimal(pNode.Element("priceEndUser").Value, CultureInfo.InvariantCulture),
                        IsVisible = true
                    };
                    product.Categories.Add(category);

                    db.Products.Add(product);
                    db.SaveChanges();

                    foreach (var prop in productXml.Descendants("property"))
                    {
                        var optionCategoryName = prop.Attribute("name").Value;
                        var optionCategory = db.OptionCategories.FirstOrDefault(c => c.Name == optionCategoryName);
                        if (optionCategory == null)
                        {
                            optionCategory = new OptionCategory
                            {
                                Name = optionCategoryName,
                                Type = OptionCategoryType.Text
                            };
                            db.OptionCategories.Add(optionCategory);
                        }

                        var optionName = prop.Element("value").Value;
                        var option = db.Options.FirstOrDefault(
                            o => o.OptionCategoryId == optionCategory.Id && o.Name == optionName);
                        if (option == null)
                        {
                            option = new Option { Category = optionCategory, Name = optionName };
                            db.Options.Add(option);
                        }

                        product.Options.Add(option);
                    }

                    var storage = HttpContext.Request.MapPath("~/Storage");
                    var wc = new WebClient();
                    foreach (var oNode in productXml.Descendants("image"))
                    {
                        var upload = new Upload
                        {
                            Type = UploadType.ProductImage,
                            Product = product
                        };
                        db.Uploads.Add(upload);
                        db.SaveChanges();

                        try
                        {
                            wc.DownloadFile(oNode.Value, Path.Combine(storage, upload.Id.ToString()));
                        }
                        catch
                        {
                            db.Uploads.Remove(upload);
                            db.SaveChanges();
                        }
                    }

                    result += productXml.Root.Element("name").Value + "<br>";
                }
                catch
                {
                }
            }

            return View(result);
        }

        [AccessAuthorize(OperatorRoles.DATABASE + OperatorRoles.WRITE)]
        public ActionResult ProductsJson()
        {
            return View();
        }

        [HttpPost]
        [AccessAuthorize(OperatorRoles.DATABASE + OperatorRoles.WRITE)]
        public ActionResult ProductsJson(HttpPostedFileBase file)
        {
            if (file.ContentLength <= 0) return View();

            var rootPath = Server.MapPath("~/Import");
            var guid = Guid.NewGuid().ToString();
            var path = Path.Combine(rootPath, guid);
            Directory.CreateDirectory(path);
            file.SaveAs(Path.Combine(rootPath, guid + ".zip"));
            using (var zip = new ZipFile(Path.Combine(rootPath, guid + ".zip")))
            {
                zip.FlattenFoldersOnExtract = true;
                zip.ExtractAll(path);
            }
            System.IO.File.Delete(Path.Combine(rootPath, guid + ".zip"));

            var imgIdMap = new Dictionary<Guid, Guid>();
            foreach (var imgFile in Directory.GetFiles(path, "*.jpg"))
            {
                var imgId = Guid.Parse(Path.GetFileNameWithoutExtension(imgFile));
                var dbUpl = db.Uploads.FirstOrDefault(u => u.Id == imgId);
                if (dbUpl == null)
                {
                    dbUpl = new Upload { Type = UploadType.ProductImage };
                    db.Uploads.Add(dbUpl);
                    db.SaveChanges();

                    System.IO.File.Copy(imgFile, Path.Combine(Server.MapPath("~/Storage"), dbUpl.Id.ToString()), true);
                }
                imgIdMap.Add(imgId, dbUpl.Id);
            }

            if (System.IO.File.Exists(Path.Combine(path, "export.json")))
            {
                // TODO: Use text reader for very large jsons
                // Import json
                /*
                using (var sr = new StreamReader(Path.Combine(path, "export.json")))
                using (var reader = new JsonTextReader(sr))
                {
                    while (reader.Read())
                    {
                        if ()
                    }
                }
                */

                dynamic json = JObject.Parse(System.IO.File.ReadAllText(Path.Combine(path, "export.json")));

                var categoryidMap = new Dictionary<int, int>();
                var optCategoryidMap = new Dictionary<int, int>();
                var optMap = new Dictionary<int, int>();
                var prodMap = new Dictionary<int, int>();

                foreach (var category in json.categories)
                {
                    int? parentId = category.parentId;
                    if (parentId != null) parentId = categoryidMap[(int)category.parentId];
                    string name = category.name;
                    var dbCategory = categoryService.FindAll()
                        .FirstOrDefault(c => c.Name == name && c.ParentId == parentId);
                    if (dbCategory == null)
                    {
                        var categoryModel = new CategoryEditViewModel
                                            {
                                                Name = category.name,
                                                Description = category.description,
                                                IsVisible = category.isVisible ?? true,
                                                SortOrder = category.sortOrder ?? 0
                                            };
                        if (category.parentId != null)
                            categoryModel.ParentId = categoryidMap[(int)category.parentId];
                        dbCategory = categoryService.AddOrUpdate(categoryModel);
                    }
                    categoryidMap.Add((int)category.id, dbCategory.Id);
                }

                foreach (var optCategory in json.optionCategories)
                {
                    string name = optCategory.name;
                    var dbOptCategory = db.OptionCategories.FirstOrDefault(c => c.Name == name);
                    if (dbOptCategory == null)
                    {
                        dbOptCategory = new OptionCategory
                                            {
                                                Name = optCategory.name,
                                                Description = optCategory.description,
                                                Type = optCategory.type,
                                                IncludeInFilters = optCategory.includeInFilters
                                            };
                        db.OptionCategories.Add(dbOptCategory);
                        db.SaveChanges();
                    }
                    optCategoryidMap.Add((int)optCategory.id, dbOptCategory.Id);
                }

                foreach (var option in json.options)
                {
                    string name = option.name;
                    int catId = optCategoryidMap[(int)option.optionCategoryId];
                    var dbOpt = db.Options.FirstOrDefault(o => o.Name == name && o.OptionCategoryId == catId);
                    if (dbOpt == null)
                    {
                        dbOpt = new Option
                                {
                                    Name = option.name,
                                    Description = option.description,
                                    OptionCategoryId = catId,
                                };
                        db.Options.Add(dbOpt);
                        db.SaveChanges();
                    }
                    optMap.Add((int)option.id, dbOpt.Id);
                }

                foreach (var product in json.products)
                {
                    string name = product.name;
                    var dbProd = productFinder.FindAll().FirstOrDefault(p => p.Name == name);
                    if (dbProd == null)
                    {
                        var prodModel = new ProductEditViewModel();
                        prodModel.Sku = product.sku;
                        prodModel.Name = product.name;
                        prodModel.Description = product.description;
                        prodModel.Price = product.price ?? 0;
                        prodModel.RetailPrice = product.retailPrice;
                        prodModel.CostPrice = product.costPrice;
                        prodModel.SalePrice = product.salePrice;
                        prodModel.IsFeatured = product.isFeatured;
                        prodModel.IsVisible = product.isVisible;
                        prodModel.CategoryIds = string.Join(",", ((JArray)product.categories).Select(
                            i => categoryidMap[(int)i].ToString()));
                        prodModel.Keywords = product.keywords;
                        prodModel.Quantity = product.quantity;
                        prodModel.TaxClassId = product.taxClassid;
                        prodModel.Weight = product.weight ?? 0;
                        prodModel.OptionIds = string.Join(",", ((JArray)product.options).Select(
                            i => optMap[(int)i].ToString()));
                        if (product.sections != null)
                        {
                            foreach (var sect in product.sections)
                            {
                                var sectModel = new ProductSectionEditViewModel
                                                {
                                                    Title = sect.title,
                                                    Type = sect.type,
                                                    Position = sect.position,
                                                    Settings = sect.settings,
                                                    Priority = sect.priority,
                                                    Text = sect.text
                                                };
                                prodModel.Sections.Add(sectModel);
                            }
                        }
                        if (product.uploads != null)
                        {
                            foreach (var upl in product.uploads)
                            {
                                if (prodModel.UploadIds == null)
                                    prodModel.UploadIds = "";
                                else
                                    prodModel.UploadIds += ",";
                                prodModel.UploadIds += imgIdMap[(Guid)upl.id];
                            }
                        }
                        if (product.skus != null)
                        {
                            foreach (var sku in product.skus)
                            {
                                var optIds = new List<int>();
                                if (sku.options != null)
                                {
                                    foreach (int optId in sku.options)
                                    {
                                        optIds.Add(optMap[optId]);
                                    }
                                }
                                var uploadIds = new List<Guid>();
                                if (sku.uploads != null)
                                {
                                    foreach (var uplId in sku.uploads)
                                    {
                                        uploadIds.Add(imgIdMap[(Guid)uplId]);
                                    }
                                }
                                prodModel.Skus.Skus.Add(
                                    new ProductSkuEditViewModel
                                    {
                                        Sku = sku.sku,
                                        Price = sku.price,
                                        Quantity = sku.quantity,
                                        UPC = sku.upc,
                                        Weight = sku.weight,
                                        OptionIds = JsonConvert.SerializeObject(optIds.ToArray()),
                                        UploadIds = JsonConvert.SerializeObject(uploadIds.ToArray())
                                    });
                            }
                        }
                        dbProd = productService.CreateOrUpdate(prodModel);
                    }
                    prodMap.Add((int)product.id, dbProd.Id);
                }
            }

            return View().WithInfo(
                "Data import has been initiated. You will receive notification as soon as the import is completed".TA());
        }

        [AccessAuthorize(OperatorRoles.DATABASE + OperatorRoles.WRITE)]
        public ActionResult TranslationsCsv()
        {
            return View();
        }

        [HttpPost]
        [AccessAuthorize(OperatorRoles.DATABASE + OperatorRoles.WRITE)]
        public ActionResult TranslationsCsv(HttpPostedFileBase file)
        {
            if (file.ContentLength <= 0) return View();

            var area = file.FileName.Contains("TranslationsAdmin") ? TranslationArea.Backend : TranslationArea.Frontend;

            var csv = new CsvReader(new StreamReader(file.InputStream));
            ImportTranslationsCsv(db, csv, area);

            return View();
        }

        public static void ImportTranslationsCsv(DataContext db, CsvReader csv, TranslationArea area)
        {
            while (csv.Read())
            {
                var code = csv.GetField<string>(0);
                var key = csv.GetField<string>(1);
                var value = csv.GetField<string>(2);

                // ToList is called on purpose for case sensitive search
                var translation =
                    db.Translations.ToList().FirstOrDefault(
                        t => t.LanguageCode == code && t.Key == key && t.Area == area);

                if (translation == null)
                {
                    translation = new Translation
                                  {
                                      LanguageCode = code,
                                      Key = key,
                                      Value = value,
                                      Area = area
                                  };
                    db.Translations.Add(translation);
                }
                else
                {
                    translation.Value = value;
                }
            }

            db.SaveChanges();

            TranslationHelper.ClearCache();
        }
    }
}