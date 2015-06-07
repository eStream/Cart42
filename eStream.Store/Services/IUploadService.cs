using System;

namespace Estream.Cart42.Web.Services
{
    public interface IUploadService
    {
        Guid? FindPrimaryIdByProduct(int productId);
    }
}