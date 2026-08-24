using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorDemoApp.Models;

[Table("SystemTransactions", Schema = "dbo")]
public class SystemTransaction
{
    [Column("Id")]
    public long Id { get; set; }

    [Column("TransactionNumber")]
    public Guid TransactionNumber { get; set; }

    [Column("CustomerEmail")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Column("Amount")]
    public decimal Amount { get; set; }

    [Column("Currency")]
    public string Currency { get; set; } = "PLN";

    [Column("PaymentMethod")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Column("Status")]
    public string Status { get; set; } = string.Empty;

    [Column("Description")]
    public string? Description { get; set; }

    [Column("TransactionDate")]
    public DateTime TransactionDate { get; set; }
}