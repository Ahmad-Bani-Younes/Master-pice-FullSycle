namespace Master_pice.ViewModel
{
    public class OrderWithPaymentViewModel
    {
        public int OrderID { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PaymentMethod { get; set; }
    }

}
