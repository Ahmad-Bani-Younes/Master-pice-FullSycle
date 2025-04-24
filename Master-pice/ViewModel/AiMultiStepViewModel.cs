namespace Master_pice.ViewModel
{
    public class AiMultiStepViewModel
    {
        public List<(string Question, List<string> Choices, string Answer)> Questions { get; set; } = new();
        public int CurrentIndex { get; set; } = 0;
        public string SelectedChoice { get; set; } = "";

        public List<(string Question, List<string> Choices, string Answer)> QuestionsRaw { get; set; }


    }


}
