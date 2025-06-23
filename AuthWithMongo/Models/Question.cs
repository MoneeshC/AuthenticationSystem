using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AuthWithMongo.Models
{
    public class Question
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public int CorrectIndex { get; set; }

    }
}
