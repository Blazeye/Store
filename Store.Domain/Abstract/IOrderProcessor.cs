using Store.Domain.Entities;

namespace Store.Domain.Abstract
{
    public interface IOrderProcessor
    {
        void ProcessOrder(Cart cart, ShippingDetails shippingdetails);
    }
}
