namespace Master_pice.Models
{
    public class Recommendation
    {
        public int RecommendationID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }

        public int? LaptopID { get; set; }
        public Laptop Laptop { get; set; }

        public int? PCID { get; set; }
        public PC PC { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
