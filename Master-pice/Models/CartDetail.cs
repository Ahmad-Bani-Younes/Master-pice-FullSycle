namespace Master_pice.Models
{
    public class CartDetail
    {
        public int ID { get; set; }
        public int CartID { get; set; }
        public Cart Cart { get; set; }

        public int? LaptopID { get; set; }
        public Laptop Laptop { get; set; }

        public int? PCID { get; set; }
        public PC PC { get; set; }

        public int? PartID { get; set; }
        public PCPart PCPart { get; set; }

        public int Quantity { get; set; }
    }

}
