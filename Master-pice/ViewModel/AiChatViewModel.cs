namespace Master_pice.ViewModel
{
    public class AiChatViewModel
    {
        public List<string> Messages { get; set; } = new();
        public string CurrentInput { get; set; }
        public string SuggestedDevice { get; set; }
    }
}
