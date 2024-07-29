namespace EmpressOfLight.Models.ViewModels
{
    public class CheckoutDetail
    {
        public List<Cart> carts = new List<Cart>();
        public List<ShippingUnit> shippingUnits = new List<ShippingUnit>();
        public List<ShippingAddress> shippingAddresses { get; set; } = new List<ShippingAddress>();
        public OrderSelect orderSelects { get; set; } = new OrderSelect();
    }
}
