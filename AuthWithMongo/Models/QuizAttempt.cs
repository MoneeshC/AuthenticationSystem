using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AuthWithMongo.Models
{
    public class QuizAttempt
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserEmail { get; set; }
        public string QuizId { get; set; }
        public DateTime AttemptedAt { get; set; }
        public int Score { get; set; }
        public int Total { get; set; }
    }
}
