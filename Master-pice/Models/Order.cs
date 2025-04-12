namespace Master_pice.Models
{
    using System;

    public class Order
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }

        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
