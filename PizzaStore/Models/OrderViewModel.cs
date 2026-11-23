namespace PizzaStore.Models
{
    public class OrderViewModel
    {
        public string PizzaName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string DeliveryTime { get; set; }
        public string OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public string CreatedAt { get; set; }
    }
}