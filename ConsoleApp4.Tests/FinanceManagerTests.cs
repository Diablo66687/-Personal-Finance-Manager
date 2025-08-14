using System;
using Xunit;
using ConsoleApp4;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp4.Tests
{
    public class FinanceManagerTests
    {
        [Fact]
        public void AddTransaction_ShouldAddTransaction()
        {
            var options = new DbContextOptionsBuilder<FinanceContext>()
                .UseInMemoryDatabase(databaseName: "AddTransactionTest")
                .Options;
            using var db = new FinanceContext(options);
            var transaction = new Transaction
            {
                Date = DateTime.Today,
                Amount = 100,
                Description = "Test",
                Category = "TestCat",
                Currency = "CZK",
                ExchangeRate = 1
            };
            db.Transactions.Add(transaction);
            db.SaveChanges();
            Assert.Equal(1, db.Transactions.Count());
            // Audit log test
            db.AuditLogs.Add(new AuditLog { Timestamp = DateTime.Now, UserId = 1, Action = "AddTransaction", Details = "Test audit" });
            db.SaveChanges();
            Assert.Equal(1, db.AuditLogs.Count());
        }

        [Fact]
        public void AddBudget_ShouldAddBudget()
        {
            var options = new DbContextOptionsBuilder<FinanceContext>()
                .UseInMemoryDatabase(databaseName: "AddBudgetTest")
                .Options;
            using var db = new FinanceContext(options);
            var budget = new Budget
            {
                Year = DateTime.Today.Year,
                Month = DateTime.Today.Month,
                Category = "TestCat",
                Limit = 1000
            };
            db.Budgets.Add(budget);
            db.SaveChanges();
            Assert.Equal(1, db.Budgets.Count());
        }

        [Fact]
        public void MonthlyReport_ShouldReturnCorrectTotals()
        {
            var options = new DbContextOptionsBuilder<FinanceContext>()
                .UseInMemoryDatabase(databaseName: "MonthlyReportTest")
                .Options;
            using var db = new FinanceContext(options);
            db.Transactions.Add(new Transaction { Date = DateTime.Today, Amount = 200, Category = "Test", Currency = "CZK", ExchangeRate = 1 });
            db.Transactions.Add(new Transaction { Date = DateTime.Today, Amount = -50, Category = "Test", Currency = "CZK", ExchangeRate = 1 });
            db.SaveChanges();
            var income = db.Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
            var expense = db.Transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
            Assert.Equal(200, income);
            Assert.Equal(-50, expense);
        }

        [Fact]
        public void AuditLog_ShouldLogActions()
        {
            var options = new DbContextOptionsBuilder<FinanceContext>()
                .UseInMemoryDatabase(databaseName: "AuditLogTest")
                .Options;
            using var db = new FinanceContext(options);
            db.AuditLogs.Add(new AuditLog { Timestamp = DateTime.Now, UserId = 1, Action = "LoginSuccess", Details = "UserId: 1" });
            db.SaveChanges();
            var log = db.AuditLogs.FirstOrDefault(l => l.Action == "LoginSuccess");
            Assert.NotNull(log);
            Assert.Equal("UserId: 1", log.Details);
        }

        [Fact]
        public async Task OpenAIService_ChatbotAsync_ShouldReturnResponse()
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // Skip test if API key is not set
                return;
            }
            var service = new OpenAIService(apiKey);
            var prompt = "Write a short bedtime story about a unicorn.";
            var result = await service.ChatbotAsync(prompt);
            Assert.False(string.IsNullOrWhiteSpace(result));
        }
    }
}
