using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorDemoApp.Models;

[Table("categories")]
public class PostgresCategory
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("slug")]
    public string Slug { get; set; } = string.Empty;
}