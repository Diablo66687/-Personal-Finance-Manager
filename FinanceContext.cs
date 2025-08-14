using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ConsoleApp4
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
    }

    public class FinanceContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; } // Audit logy

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=finance.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>().Property(t => t.Currency).HasColumnType("TEXT");
            modelBuilder.Entity<Transaction>().Property(t => t.ExchangeRate).HasColumnType("TEXT");
        }

        // Demo data for showcase
        public static void SeedDemoData()
        {
            using var db = new FinanceContext();
            db.Database.EnsureCreated();

            // Users
            if (!db.Users.Any())
            {
                db.Users.AddRange(new List<User>
                {
                    new User { Username = "demo", PasswordHash = PasswordHelper.HashPassword("demo"), DisplayName = "Demo User", Role = "user" },
                    new User { Username = "admin", PasswordHash = PasswordHelper.HashPassword("admin"), DisplayName = "Admin", Role = "admin" }
                });
                db.SaveChanges();
            }

            // Transactions
            if (!db.Transactions.Any())
            {
                db.Transactions.AddRange(new List<Transaction>
                {
                    new Transaction { Date = DateTime.Today.AddDays(-10), Amount = 15000, Description = "V�plata", Category = "P��jem", Currency = "CZK", ExchangeRate = 1 },
                    new Transaction { Date = DateTime.Today.AddDays(-9), Amount = -1200, Description = "Potraviny", Category = "J�dlo", Currency = "CZK", ExchangeRate = 1 },
                    new Transaction { Date = DateTime.Today.AddDays(-8), Amount = -800, Description = "Restaurace", Category = "J�dlo", Currency = "CZK", ExchangeRate = 1 },
                    new Transaction { Date = DateTime.Today.AddDays(-7), Amount = -500, Description = "MHD", Category = "Doprava", Currency = "CZK", ExchangeRate = 1 },
                    new Transaction { Date = DateTime.Today.AddDays(-6), Amount = -2000, Description = "N�jem", Category = "Bydlen�", Currency = "CZK", ExchangeRate = 1 },
                    new Transaction { Date = DateTime.Today.AddDays(-5), Amount = -300, Description = "Kino", Category = "Z�bava", Currency = "CZK", ExchangeRate = 1 },
                    new Transaction { Date = DateTime.Today.AddDays(-4), Amount = 100, Description = "Z�loha od kamar�da", Category = "P��jem", Currency = "CZK", ExchangeRate = 1 },
                    new Transaction { Date = DateTime.Today.AddDays(-3), Amount = -250, Description = "K�va", Category = "J�dlo", Currency = "CZK", ExchangeRate = 1 },
                    new Transaction { Date = DateTime.Today.AddDays(-2), Amount = -1000, Description = "Internet", Category = "Bydlen�", Currency = "CZK", ExchangeRate = 1 },
                    new Transaction { Date = DateTime.Today.AddDays(-1), Amount = -400, Description = "Taxi", Category = "Doprava", Currency = "CZK", ExchangeRate = 1 }
                });
                db.SaveChanges();
            }

            // Budgets
            if (!db.Budgets.Any())
            {
                db.Budgets.AddRange(new List<Budget>
                {
                    new Budget { Year = DateTime.Today.Year, Month = DateTime.Today.Month, Category = "J�dlo", Limit = 3000 },
                    new Budget { Year = DateTime.Today.Year, Month = DateTime.Today.Month, Category = "Doprava", Limit = 1500 },
                    new Budget { Year = DateTime.Today.Year, Month = DateTime.Today.Month, Category = "Bydlen�", Limit = 5000 },
                    new Budget { Year = DateTime.Today.Year, Month = DateTime.Today.Month, Category = "Z�bava", Limit = 1000 }
                });
                db.SaveChanges();
            }

            // Goals
            if (!db.Goals.Any())
            {
                db.Goals.Add(new Goal { UserId = 1, Name = "Dovolen�", TargetAmount = 20000, CurrentAmount = 5000, Deadline = DateTime.Today.AddMonths(6) });
                db.SaveChanges();
            }

            // Recurring Transactions
            if (!db.RecurringTransactions.Any())
            {
                db.RecurringTransactions.Add(new RecurringTransaction {
                    StartDate = DateTime.Today.AddMonths(-2),
                    Amount = -2000,
                    Description = "N�jem",
                    Category = "Bydlen�",
                    Frequency = "monthly",
                    DayOfMonth = 1
                });
                db.SaveChanges();
            }
        }
    }
}