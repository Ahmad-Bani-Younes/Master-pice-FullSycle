namespace Master_pice.Models
{
    public class Laptop
    {
        public int LaptopID { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Processor { get; set; }
        public string RAM { get; set; }
        public string Storage { get; set; }
        public string GPU { get; set; }
        public decimal ScreenSize { get; set; }
        public decimal Price { get; set; }
        public string ImageURL { get; set; }
        public string Description { get; set; }
        public int Stock { get; set; }
        public string AdditionalImageURLs { get; set; }

        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

    }

}
