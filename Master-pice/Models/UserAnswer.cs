using System.ComponentModel.DataAnnotations;

namespace Master_pice.Models
{
    public class UserAnswer
    {

        [Key]
        public int AnswerID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }

        public int QuestionID { get; set; }
        public Question Question { get; set; }

        public string SelectedOption { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
