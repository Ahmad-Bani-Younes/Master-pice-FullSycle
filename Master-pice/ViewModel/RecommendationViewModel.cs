using Master_pice.Models;

namespace Master_pice.ViewModel
{
    public class RecommendationViewModel
    {
        public Dictionary<int, string> SelectedAnswers { get; set; } = new();
        public List<Question> Questions { get; set; } = new();
        public object? RecommendedDevice { get; set; }


    }


}
