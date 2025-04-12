namespace Master_pice.Models
{
    public class Cart
    {
        public int CartID { get; set; }

        public int UserID { get; set; }
        public User User { get; set; }

        public int ProductId { get; set; }     
        public string Type { get; set; }       
        public int Quantity { get; set; }      
    }
}
