namespace Master_pice.Models
{
    using System;

    public class Review
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }

        public int? LaptopID { get; set; }
        public Laptop Laptop { get; set; }

        public int? PCID { get; set; }
        public PC PC { get; set; }

        public int? PartID { get; set; }
        public PCPart PCPart { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
