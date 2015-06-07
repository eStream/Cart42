using System;

namespace Estream.Cart42.Web.Services
{
    public interface IVisitorService
    {
        Guid TrackVisitor(Guid? id, string userId, string ipAddress);
    }
}