using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;
using Glimpse.AspNet.Tab;

namespace Estream.Cart42.Web.Controllers
{
    public class UploadController : BaseController
    {
        public UploadController(DataContext db)
            : base(db)
        {
        }

        [ChildActionOnly]
        public ActionResult Thumbnail(int productId, string viewName)
        {
            Upload upload = db.Uploads.FirstOrDefault(u => u.ProductId == productId);

            string imageUrl = "/Content/img/no-img-gallery.png";
            if (upload != null)
                imageUrl = "/api/Upload/" + upload.Id;

            return PartialView(viewName, imageUrl);
        }

        [ChildActionOnly]
        public ActionResult Photos(int productId, string viewName)
        {
            IQueryable<Upload> uploads = db.Uploads.Where(u => u.ProductId == productId);

            return PartialView(viewName, uploads.ToList());
        }

        [OutputCache(Duration = 5 * 60, VaryByParam = "*", Location = OutputCacheLocation.Client)]
        public ActionResult View(string id, int? productId = null, int? width = null, int? height = null,
            bool crop = false)
        {
            string path;
            if (productId.HasValue)
            {
                var idGuid = DataContext.Current.Uploads.Where(u => u.ProductId == productId)
                    .OrderBy(u => u.SortOrder).Select(u => u.Id)
                    .FirstOrDefault();
                if (idGuid != default(Guid)) id = idGuid.ToString();
            }

            string root = ControllerContext.HttpContext.Server.MapPath("~/Storage");
            if (string.IsNullOrEmpty(id) || id == Guid.Empty.ToString())
            {
                id = Guid.Empty.ToString();
                path = ControllerContext.HttpContext.Server.MapPath("~/Content/img/no-img.jpg");
            }
            else
            {
                path = Path.Combine(root, id);
            }

            if (width.HasValue || height.HasValue)
            {
                var cachePath = Path.Combine(root,
                    string.Format("{0}_{1}_{2}{3}", id, width, height, crop ? "_crop" : ""));
                if (!System.IO.File.Exists(cachePath))
                {
                    using (Image original = Image.FromFile(path))
                    {
                        using (Image resized = original.Resize(width ?? int.MaxValue, height ?? int.MaxValue, crop))
                        {
                            resized.Save(cachePath, ImageFormat.Jpeg);
                        }
                    }
                }
                path = cachePath;
            }

            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var result = new FileStreamResult(stream, "image/jpg");
            return result;
        }
    }
}