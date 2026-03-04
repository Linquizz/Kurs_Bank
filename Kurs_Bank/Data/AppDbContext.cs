using System.Data.Entity;
using static System.Data.Entity.Migrations.Model.UpdateDatabaseOperation;
using Kurs_Bank.Models;

namespace Kurs_Bank.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=BankDB")
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Credit> Credits { get; set; }
        public DbSet<CreditPayment> CreditPayments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Transaction_Account> TransactionAccounts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>()
                .HasOptional(c => c.Users)
                .WithRequired(u => u.Client);

            modelBuilder.Entity<Account>()
                .HasRequired(a => a.Client)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.PassportNumber)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Credit>()
                .HasRequired(c => c.Client)
                .WithMany(cl => cl.Credits)
                .HasForeignKey(c => c.PassportNumber)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Credit>()
                .HasRequired(c => c.Account)
                .WithMany(a => a.Credits)
                .HasForeignKey(c => c.AccountID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Card>()
                .HasRequired(c => c.Account)
                .WithMany(a => a.Cards)
                .HasForeignKey(c => c.AccountID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CreditPayment>()
                .HasRequired(p => p.Credit)
                .WithMany(c => c.CreditPayments)
                .HasForeignKey(p => p.CreditID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Transaction_Account>()
                .HasRequired(ta => ta.Transaction)
                .WithMany(t => t.TransactionAccounts)
                .HasForeignKey(ta => ta.TransactionID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Transaction_Account>()
                .HasRequired(ta => ta.Account)
                .WithMany(a => a.TransactionAccounts)
                .HasForeignKey(ta => ta.AccountID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Login)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Phone)
                .IsUnique();

            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(15, 2);

            modelBuilder.Entity<Credit>()
                .Property(c => c.Amount)
                .HasPrecision(15, 2);

            modelBuilder.Entity<Credit>()
                .Property(c => c.InterestRate)
                .HasPrecision(5, 2);

            modelBuilder.Entity<CreditPayment>()
                .Property(p => p.Amount)
                .HasPrecision(15, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(15, 2);
        }
    }
}