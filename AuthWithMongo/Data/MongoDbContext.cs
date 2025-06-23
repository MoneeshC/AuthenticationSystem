using AuthWithMongo.Data;
using AuthWithMongo.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AuthWithMongo.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Quiz> Quizzes => _database.GetCollection<Quiz>("Quizzes");
        public IMongoCollection<QuizAttempt> QuizAttempts => _database.GetCollection<QuizAttempt>("QuizAttempts");
    }
}