using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using CsvHelper;
using Estream.Cart42.Web.Controllers;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Estream.Cart42.Web.Services;

namespace Estream.Cart42.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class DataExportController : BaseController
    {
        private readonly IWorkProcessService workProcessService;

        public DataExportController(DataContext db, IWorkProcessService workProcessService)
            : base(db)
        {
            this.workProcessService = workProcessService;
        }

        // GET: Admin/DataExport
        [AccessAuthorize(OperatorRoles.DATABASE)]
        public ActionResult Index()
        {
            return View();
        }

        [AccessAuthorize(OperatorRoles.DATABASE)]
        public ActionResult ProductsJson()
        {
            return View();
        }

        [AccessAuthorize(OperatorRoles.DATABASE)]
        public ActionResult ProductsJsonStart()
        {
            var exportPath = Server.MapPath("/Export");

            var wpId = workProcessService.Add(WorkProcessType.Export);
            var storagePath = Server.MapPath("/Storage");
            ThreadPool.QueueUserWorkItem(s =>
                new DataExportService().ExportProductsJson(exportPath, storagePath, wpId), null);

            return JsonSuccess<string>(null);
        }

        [AccessAuthorize(OperatorRoles.DATABASE)]
        public ActionResult ProductExportsList()
        {
            var exportPath = Server.MapPath("/Export");
            var files = Directory.GetFiles(exportPath, "Products_*.zip");


            return JsonSuccess(files.Select(f =>
                new
                {
                    Name = Path.GetFileName(f),
                    Url = "/Export/" + Path.GetFileName(f)
                }));
        }

        [AccessAuthorize(OperatorRoles.DATABASE)]
        public FileStreamResult TranslationsCsv(bool admin = false)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var csv = new CsvWriter(sw);

            var translations = (from t in db.Translations
                where t.Area == (admin ? TranslationArea.Backend : TranslationArea.Frontend)
                select new
                       {
                           t.LanguageCode,
                           t.Key,
                           t.Value
                       }).ToList();

            csv.WriteHeader(translations.First().GetType());
            foreach (var trans in translations)
            {
                csv.WriteRecord(trans);
            }

            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            var result = new FileStreamResult(ms, "application/octet-stream");
            result.FileDownloadName = "Translations_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".csv";
            return result;
        }
    }
}