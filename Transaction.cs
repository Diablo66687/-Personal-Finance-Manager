namespace ConsoleApp4
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        // Pøidat podporu mìny
        public string Currency { get; set; } // ISO kód mìny, napø. CZK, EUR, USD
        public decimal? ExchangeRate { get; set; } // kurz vùèi základní mìnì (napø. CZK)
    }
}
