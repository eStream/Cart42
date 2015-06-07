using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;
using Glimpse.Core.Extensions;

namespace Estream.Cart42.Web.Services
{
    public class UploadService : IUploadService
    {
        private readonly DataContext db;
        private readonly ICacheService cacheService;

        public UploadService(DataContext db, ICacheService cacheService)
        {
            this.db = db;
            this.cacheService = cacheService;
        }

        public Guid? FindPrimaryIdByProduct(int productId)
        {
            var id = cacheService.Get(string.Format("FindPrimaryByProduct({0})", productId),
                () =>
                {
                    var uplId = db.Uploads.Where(u => u.ProductId == productId && u.Type == UploadType.ProductImage)
                        .OrderBy(u => u.SortOrder)
                        .Select(u => u.Id).FirstOrDefault();
                    if (uplId == default(Guid)) return null;
                    return (object) uplId;
                }, absoluteExpiration: DateTime.Now.AddMinutes(30));

            return id is Guid ? (Guid?)id : null;
        }
    }
}