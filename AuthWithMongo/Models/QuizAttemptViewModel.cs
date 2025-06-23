namespace AuthWithMongo.Models
{
    public class QuizAttemptViewModel
    {
        public string QuizId { get; set; }
        public string Title { get; set; }
        public List<QuestionViewModel> Questions { get; set; }
        public List<int> Answers { get; set; } // user’s selected option per question
        public int TimeLimitSeconds { get; set; } // user’s time limit for the quiz

    }
}
