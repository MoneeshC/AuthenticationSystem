namespace AuthWithMongo.Models
{
    public class QuizAttemptStatsViewModel
    {
        public string QuizTitle { get; set; }
        public List<QuizAttempt> Attempts { get; set; }

    }
}
