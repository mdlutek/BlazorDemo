using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorDemoApp.Models;

[Table("audit_logs")]
public class PostgresAuditLog
{
    [Column("id")]
    public long Id { get; set; }

    [Column("table_name")]
    public string TableName { get; set; } = string.Empty;

    [Column("operation")]
    public string Operation { get; set; } = string.Empty;

    [Column("record_id")]
    public int? RecordId { get; set; }

    // Dane zapisane przez Trigger w formacie JSONB
    [Column("old_data", TypeName = "jsonb")]
    public string? OldData { get; set; }

    [Column("new_data", TypeName = "jsonb")]
    public string? NewData { get; set; }

    [Column("performed_at")]
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
}