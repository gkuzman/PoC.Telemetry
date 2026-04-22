namespace FPCC.DB.Models;

public class Withdrawal
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string Iban { get; set; } = null!;
}