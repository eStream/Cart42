using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using CsvHelper;
using Elmah;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using JsonTextWriter = Newtonsoft.Json.JsonTextWriter;

namespace Estream.Cart42.Web.Services
{
    public class DataExportService
    {
        private readonly DataContext db;
        private readonly ICategoryService categoryService;
        private readonly IProductFinder productFinder;
        private readonly IWorkProcessService workProcessService;

        public DataExportService()
        {
            // Not using structuremap as dependencies are being disposed too early

            db = DataContext.Create();
            categoryService = new CategoryService(db, null, new CacheService());
            productFinder = new ProductFinder(db, categoryService);
            workProcessService = new WorkProcessService(db);
        }

        public void ExportProductsJson(string path, string storagePath, int workProcessID, bool zipIt = true)
        {
            try
            {
                bool cancelRequested;
                workProcessService.Update(workProcessID, "Export started", 0);

                var totalAll = db.Categories.Count() + db.Products.Count() +
                               db.OptionCategories.Count() + db.Options.Count() + 
                               db.Uploads.Count();
                var countAll = 0;

                var fileName = "Products_" + DateTime.Now.ToString("yyyyMMddTHHmmss");

                var serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.Formatting = Formatting.Indented;
                serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();

                using (var sw = new StreamWriter(Path.Combine(path, fileName + ".json")))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.WriteStartObject();

                    #region Write categories 

                    writer.WritePropertyName("categories");
                    writer.WriteStartArray();

                    var count = 0;
                    var total = categoryService.FindAll().Count();
                    foreach (var category in categoryService.FindAll().OrderBy(c => c.Id).InChunksOf(100))
                    {
                        serializer.Serialize(writer, new
                                                     {
                                                         category.Id,
                                                         category.Name,
                                                         category.ParentId,
                                                         category.Description,
                                                         category.IsVisible,
                                                         SortOrder = category.SortOrder.AsNullIfDefault()
                                                     });

                        count++;
                        countAll++;
                        if (count%100 == 0)
                        {
                            workProcessService.Update(workProcessID,
                                string.Format("Exporting product categories ({0} of {1})", count, total),
                                ((double) countAll/totalAll)*100.0, out cancelRequested);
                            if (cancelRequested) return;
                        }
                    }

                    writer.WriteEndArray();

                    #endregion

                    #region Write option categories

                    writer.WritePropertyName("optionCategories");
                    writer.WriteStartArray();

                    count = 0;
                    total = db.OptionCategories.Count();
                    foreach (var category in db.OptionCategories.OrderBy(c => c.Id).InChunksOf(100))
                    {
                        serializer.Serialize(writer, new
                                                     {
                                                         category.Id,
                                                         category.Name,
                                                         category.Description,
                                                         category.Type,
                                                         category.IncludeInFilters
                                                     });

                        count++;
                        countAll++;
                        if (count%100 == 0)
                        {
                            workProcessService.Update(workProcessID,
                                string.Format("Exporting option categories ({0} of {1})", count, total),
                                ((double)countAll / totalAll) * 100.0, out cancelRequested);
                            if (cancelRequested) return;
                        }
                    }

                    writer.WriteEndArray();

                    #endregion

                    #region Write options

                    writer.WritePropertyName("options");
                    writer.WriteStartArray();

                    count = 0;
                    total = db.Options.Count();
                    foreach (var option in db.Options.OrderBy(o => o.Id).InChunksOf(100))
                    {
                        serializer.Serialize(writer, new
                                                     {
                                                         option.Id,
                                                         option.Name,
                                                         option.OptionCategoryId,
                                                         option.Description,
                                                     });

                        count++;
                        countAll++;
                        if (count%100 == 0)
                        {
                            workProcessService.Update(workProcessID,
                                string.Format("Exporting options ({0} of {1})", count, total),
                                ((double)countAll / totalAll) * 100.0, out cancelRequested);
                            if (cancelRequested) return;
                        }
                    }

                    writer.WriteEndArray();

                    #endregion

                    #region Write products

                    writer.WritePropertyName("products");
                    writer.WriteStartArray();

                    count = 0;
                    total = productFinder.FindAll().Count();
                    foreach (var product in productFinder.FindAll().OrderBy(p => p.Id).InChunksOf(100))
                    {
                        serializer.Serialize(writer, new
                                                     {
                                                         product.Id,
                                                         product.Sku,
                                                         product.Name,
                                                         product.Price,
                                                         product.CostPrice,
                                                         product.RetailPrice,
                                                         product.SalePrice,
                                                         Weight = product.Weight.AsNullIfDefault(),
                                                         product.Quantity,
                                                         product.Description,
                                                         product.Keywords,
                                                         product.IsFeatured,
                                                         product.IsVisible,
                                                         Categories = product.Categories.Select(c => c.Id).ToArray(),
                                                         TaxClass =
                                                             product.TaxClass != null ? product.TaxClass.Name : null,
                                                         Sections = product.Sections.Select(s =>
                                                             new
                                                             {
                                                                 s.Title,
                                                                 s.Type,
                                                                 s.Position,
                                                                 s.Settings,
                                                                 s.Priority
                                                             }).ToArray(),
                                                         Options = product.Options.Select(o => o.Id).ToArray(),
                                                         Uploads = product.Uploads.Select(u =>
                                                             new
                                                             {
                                                                 u.Id,
                                                                 u.SortOrder,
                                                                 u.Type
                                                             }).ToArray(),
                                                         Skus = product.Skus.Select(s =>
                                                             new
                                                             {
                                                                 s.Sku,
                                                                 s.UPC,
                                                                 s.Quantity,
                                                                 s.Price,
                                                                 s.Weight,
                                                                 Options = s.Options.Select(o => o.Id).ToArray(),
                                                                 Uploads = s.Uploads.Select(u => u.Id).ToArray()
                                                             })
                                                     });

                        count++;
                        countAll++;
                        if (count%100 == 0)
                        {
                            workProcessService.Update(workProcessID,
                                string.Format("Exporting products ({0} of {1})", count, total),
                                ((double)countAll / totalAll) * 100.0, out cancelRequested);
                            if (cancelRequested) return;
                        }
                    }

                    writer.WriteEndArray();

                    #endregion

                    writer.WriteEndObject();
                }

                if (zipIt)
                {
                    using (var zip = new ZipFile(Path.Combine(path, fileName + ".zip")))
                    {
                        zip.AddFile(Path.Combine(path, fileName + ".json"), "").FileName = "Export.json";
                        zip.Save();

                        foreach (var upload in db.Uploads.Where(u => u.Type == UploadType.ProductImage)
                            .OrderBy(u => u.Id).InChunksOf(100))
                        {
                            zip.AddFile(Path.Combine(storagePath, upload.Id.ToString()), "")
                                .FileName = upload.Id + ".jpg";
                        }
                        zip.SaveProgress += delegate(object sender, SaveProgressEventArgs args)
                        {
                            if (args.EntriesSaved == 0 || args.EntriesSaved % 100 != 0) return;
                            workProcessService.Update(workProcessID,
                                string.Format("Archiving photos ({0} of {1})", args.EntriesSaved,
                                    args.EntriesTotal),
                                ((double)(countAll + args.EntriesSaved) / totalAll) * 100.0,
                                out cancelRequested);
                            if (cancelRequested) args.Cancel = true;
                        };
                        zip.Save();
                    }

                    File.Delete(Path.Combine(path, fileName + ".json"));
                }

                workProcessService.Update(workProcessID, "Export completed", 100, isComplete: true);
            }
            catch (Exception err)
            {
                try
                {
                    workProcessService.Update(workProcessID, "Export failed", 100, isComplete: true, error: err.Message);
                }
                catch
                {
                    // Probably db issue; do not throw exception to avoid thread kill
                }
            }
        }

        public void ExportPhotos(string path, string storagePath, int workProcessID)
        {
            try
            {
                workProcessService.Update(workProcessID, "Export started", 0);

                var fileName = "Photos_" + DateTime.Now.ToString("yyyyMMddTHHmmss");
                using (var zip = new ZipFile(Path.Combine(path, fileName + ".zip")))
                {
                    foreach (var upload in db.Uploads.Where(u => u.Type == UploadType.ProductImage)
                        .OrderBy(u => u.Id).InChunksOf(100))
                    {
                        zip.AddFile(Path.Combine(storagePath, upload.Id.ToString()), "")
                            .FileName = upload.Id + ".jpg";
                    }
                    zip.SaveProgress += delegate(object sender, SaveProgressEventArgs args)
                                        {
                                            if (args.EntriesSaved == 0 || args.EntriesSaved%100 != 0) return;
                                            bool cancelRequested;
                                            workProcessService.Update(workProcessID,
                                                string.Format("Archiving photos ({0} of {1})", args.EntriesSaved,
                                                    args.EntriesTotal),
                                                ((double) args.EntriesSaved/args.EntriesTotal)*100.0,
                                                out cancelRequested);
                                            if (cancelRequested) args.Cancel = true;
                                        };
                    zip.Save();
                }

                workProcessService.Update(workProcessID, "Export completed", 100, isComplete: true);
            }
            catch (Exception err)
            {
                try
                {
                    workProcessService.Update(workProcessID, "Export failed", 100, isComplete: true, error: err.Message);
                }
                catch
                {
                }

                try
                {
                    ErrorLog.GetDefault(HttpContext.Current).Log(new Error(err));
                }
                catch
                {
                }
            }
        }

        public void ExportCategories(string path, bool zipIt = true)
        {
            var fileName = "Categories_" + DateTime.Now.ToString("yyyyMMddTHHmmss");
            using (var file = File.CreateText(Path.Combine(path, fileName + ".csv")))
            {
                var csv = new CsvWriter(file);
                csv.Configuration.RegisterClassMap<CategoryCsvMap>();
                csv.WriteHeader<Category>();

                foreach (var category in categoryService.FindAll().OrderBy(c => c.Id).InChunksOf(100))
                {
                    csv.WriteRecord(category);
                }
            }

            if (zipIt)
            {
                using (var zip = new ZipFile(Path.Combine(path, fileName + ".zip")))
                {
                    zip.AddFile(Path.Combine(path, fileName + ".csv"), "").FileName = "Categories.csv";
                    zip.Save();
                }

                File.Delete(Path.Combine(path, fileName + ".csv"));
            }
        }

        public void ExportProducts(string path, bool zipIt = true)
        {
            var fileName = "Products_" + DateTime.Now.ToString("yyyyMMddTHHmmss");
            using (var file = File.CreateText(Path.Combine(path, fileName + ".csv")))
            {
                var csv = new CsvWriter(file);
                csv.Configuration.RegisterClassMap<ProductCsvMap>();
                csv.WriteField("Category");
                csv.WriteHeader<Product>();

                foreach (var product in productFinder.FindAll().OrderBy(p => p.Id).InChunksOf(100))
                {
                    csv.WriteField(product.Categories.First().Name);
                    csv.WriteRecord(product);
                }
            }

            if (zipIt)
            {
                using (var zip = new ZipFile(Path.Combine(path, fileName + ".zip")))
                {
                    zip.AddFile(Path.Combine(path, fileName + ".csv"), "").FileName = "Products.csv";
                    zip.Save();
                }

                File.Delete(Path.Combine(path, fileName + ".csv"));
            }
        }

    }
}