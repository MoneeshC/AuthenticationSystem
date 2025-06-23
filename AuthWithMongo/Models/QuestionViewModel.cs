namespace AuthWithMongo.Models
{
    public class QuestionViewModel
    {
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public int Index { get; set; } // question number in the quiz

    }
}
