using System.ComponentModel.DataAnnotations;

namespace Master_pice.Models
{
    public class PCPart
    {
        [Key]
        public int PartID { get; set; }

        public string Category { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Compatibility { get; set; }
        public decimal Price { get; set; }
        public string ImageURL { get; set; }
        public int Stock { get; set; }
        public string AdditionalImageURLs { get; set; }
    }
}
