namespace PAM.DB.Models;

public class Wallet
{
    // PK and FK — enforces the 1-to-1 relationship with Account
    public int AccountId { get; set; }
    public decimal Balance { get; set; }
    public decimal Reserved { get; set; }

    // Navigation property back to Account
    public Account Account { get; set; } = null!;
}