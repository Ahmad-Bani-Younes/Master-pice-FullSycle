namespace Master_pice.ViewModel
{
    public class StepQuestionViewModel
    {
        public int CurrentStep { get; set; }
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public List<string> Options { get; set; } = new();
        public string SelectedAnswer { get; set; }
        public int TotalSteps { get; set; }

    }

}
