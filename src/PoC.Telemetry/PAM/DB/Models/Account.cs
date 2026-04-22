namespace PAM.DB.Models;

public class Account
{
    public int Id { get; set; }

    // Navigation property – one Account has one Wallet
    public Wallet? Wallet { get; set; }
}