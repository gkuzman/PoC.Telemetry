using FPCC.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace FPCC.DB;

public class FpccDbContext(DbContextOptions<FpccDbContext> options) : DbContext(options)
{
    public DbSet<Withdrawal> Withdrawals => Set<Withdrawal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Withdrawal>()
            .Property(w => w.Amount).HasPrecision(18, 4);
    }
}

