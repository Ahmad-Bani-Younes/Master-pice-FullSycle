namespace Master_pice.Models
{
    public class Payment
    {
        public int ID { get; set; }

        public int UserID { get; set; }
        public User User { get; set; }

        public int OrderID { get; set; }
        public Order Order { get; set; }

        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }

}
