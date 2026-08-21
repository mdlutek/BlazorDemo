using System.Text.Json.Serialization;

namespace BlazorDemoApp.Models;

public class CosmosNote
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // Klucz partycji (Partition Key) w Cosmos DB
    [JsonPropertyName("category")]
    public string Category { get; set; } = "Ogólne";

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "Medium";

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}