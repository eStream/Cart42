using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.DependencyResolution.Filters;
using Estream.Cart42.Web.Domain;
using Estream.Cart42.Web.Helpers;

namespace Estream.Cart42.Web.Controllers.Api
{
    public class UploadController : ApiController
    {
        // GET: api/Upload/5
        public HttpResponseMessage Get(string id, int? productId = null, int? width = null, int? height = null,
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
            if (id == Guid.Empty.ToString())
            {
                path = HttpContext.Current.Server.MapPath("~/Content/img/no-img-gallery.png");
            }
            else
            {
                string root = HttpContext.Current.Server.MapPath("~/Storage");

                path = Path.Combine(root, id);
                if (width.HasValue || height.HasValue)
                {
                    path = Path.Combine(root, string.Format("{0}_{1}_{2}{3}", id, width, height, crop ? "_crop" : ""));
                    if (!File.Exists(path))
                    {
                        if (File.Exists(Path.Combine(root, id)))
                        {
                            using (Image original = Image.FromFile(Path.Combine(root, id)))
                            {
                                using (Image resized = original.Resize(width ?? int.MaxValue, height ?? int.MaxValue, crop))
                                {
                                    resized.Save(path, ImageFormat.Jpeg);
                                }
                            }
                        }
                        else
                        {
                            // Return default image
                            return new HttpResponseMessage(HttpStatusCode.NotFound);
                        }
                    }
                }
            }

            var result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("image/jpg");
            return result;
        }

        // POST: api/Upload
        [AdminAuthorize]
        public async Task<HttpResponseMessage> Post()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("~/Storage");
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                var guids = new List<string>();

                foreach (MultipartFileData file in provider.FileData)
                {
                    var upload = new Upload
                                 {
                                     Type = UploadType.ProductImage,
                                 };
                    DataContext.Current.Uploads.Add(upload);
                    DataContext.Current.SaveChanges();

                    File.Move(file.LocalFileName, Path.Combine(root, upload.Id.ToString()));

                    guids.Add(upload.Id.ToString());
                }
                return Request.CreateResponse(HttpStatusCode.OK, guids[0]);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        // DELETE: api/Upload/5
        [AdminAuthorize]
        public void Delete(int id)
        {
        }
    }
}