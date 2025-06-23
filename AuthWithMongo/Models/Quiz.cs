using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AuthWithMongo.Models
{
    public class Quiz
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; } // New property for image
        public List<Question> Questions { get; set; }
        public int TimeLimitSeconds { get; set; }

    }
}
