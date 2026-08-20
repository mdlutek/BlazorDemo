using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlazorDemoApp.Models;

public class MongoProduct
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    // Siła NoSQL: Lista tagów jako tablica w dokumencie JSON
    public List<string> Tags { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}