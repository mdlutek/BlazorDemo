namespace BlazorDemo.ItsmAiAgent.Api.Models;

public class TicketModel
{
    [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString()[..8];

    [Newtonsoft.Json.JsonProperty(PropertyName = "category")]
    [System.Text.Json.Serialization.JsonPropertyName("category")]
    public string Category { get; set; } = "General";

    [Newtonsoft.Json.JsonProperty(PropertyName = "title")]
    [System.Text.Json.Serialization.JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [Newtonsoft.Json.JsonProperty(PropertyName = "description")]
    [System.Text.Json.Serialization.JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [Newtonsoft.Json.JsonProperty(PropertyName = "status")]
    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string Status { get; set; } = "Open";

    [Newtonsoft.Json.JsonProperty(PropertyName = "priority")]
    [System.Text.Json.Serialization.JsonPropertyName("priority")]
    public string Priority { get; set; } = "Medium";

    [Newtonsoft.Json.JsonProperty(PropertyName = "resolution")]
    [System.Text.Json.Serialization.JsonPropertyName("resolution")]
    public string? Resolution { get; set; }

    [Newtonsoft.Json.JsonProperty(PropertyName = "embedding")]
    [System.Text.Json.Serialization.JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }
}