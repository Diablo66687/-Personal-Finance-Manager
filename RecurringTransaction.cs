using System;

namespace ConsoleApp4
{
    public class RecurringTransaction
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Frequency { get; set; } // "monthly", "weekly"
        public int? DayOfMonth { get; set; } // for monthly
        public DayOfWeek? DayOfWeek { get; set; } // for weekly
    }
}
