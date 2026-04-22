using Microsoft.EntityFrameworkCore;
using PAM.DB.Models;

namespace PAM.DB;

public class PamDbContext(DbContextOptions<PamDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Wallet> Wallets => Set<Wallet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Wallet>()
            .HasKey(w => w.AccountId);

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Wallet)
            .WithOne(w => w.Account)
            .HasForeignKey<Wallet>(w => w.AccountId);
    }
}

