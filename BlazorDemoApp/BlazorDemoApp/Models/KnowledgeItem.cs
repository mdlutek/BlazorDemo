using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorDemoApp.Models;

public class KnowledgeItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = "C# / .NET";

    // Natywna tablica Postgresa: text[]
    public List<string> Tags { get; set; } = new();

    // Kolumna typu JSONB - zagnieżdżony dokument wewnątrz tabeli SQL!
    [Column(TypeName = "jsonb")]
    public ItemMetadata Metadata { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ItemMetadata
{
    public string Author { get; set; } = "Mateusz";
    public int Rating { get; set; } = 5;
    public string Environment { get; set; } = "Cloud (Azure)";
    public bool IsVerified { get; set; } = true;
}