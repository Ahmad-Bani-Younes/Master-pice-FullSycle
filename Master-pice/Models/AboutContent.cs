using System.ComponentModel.DataAnnotations;

namespace Master_pice.Models
{
    public class AboutContent
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
