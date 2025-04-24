using System.ComponentModel.DataAnnotations;

namespace Master_pice.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class User
    {
        public int ID { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Phone { get; set; }

        [Required]
        public string UserType { get; set; } 

        public string ProfileImage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Region { get; set; }

    }

}
