namespace Estream.Cart42.Web.Services
{
    public interface IDeleterService
    {
        void DeleteProduct(int id);
        void DeleteProductSku(int id);
        void DeleteOrderItem(int id);
        void DeleteShoppingCartItem(int id);
        void DeleteReturnItem(int id);
        void DeleteShipmentItem(int id);
        void DeleteOption(int id);
        void DeleteBlog(int id);
        void DeleteBlogPost(int id);
        void DeleteBlogPostComment(int id);
        void DeleteOrder(int id);
        void DeleteShipment(int id);
        void DeleteReturn(int id);
    }
}