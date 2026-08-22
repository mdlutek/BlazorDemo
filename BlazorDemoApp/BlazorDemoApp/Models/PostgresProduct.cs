using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorDemoApp.Models;

[Table("products")]
public class PostgresProduct
{
    [Column("id")]
    public int Id { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("sku")]
    public string Sku { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("price")]
    public decimal Price { get; set; }

    [Column("stock_quantity")]
    public int StockQuantity { get; set; }

    // Natywna tablica Postgresa: text[]
    [Column("tags")]
    public List<string> Tags { get; set; } = new();

    // Zagnieżdżony JSONB w Postgresie
    [Column("specifications", TypeName = "jsonb")]
    public ProductSpecs Specifications { get; set; } = new();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relacja FK do tabeli categories
    [ForeignKey("CategoryId")]
    public PostgresCategory? Category { get; set; }
}

public class ProductSpecs
{
    public string Cpu { get; set; } = string.Empty;
    public string Ram { get; set; } = string.Empty;
    public string Color { get; set; } = "Graphite";
    public string Warranty { get; set; } = "24m";
}