namespace ConsoleApp4
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        // P�idat podporu m�ny
        public string Currency { get; set; } // ISO k�d m�ny, nap�. CZK, EUR, USD
        public decimal? ExchangeRate { get; set; } // kurz v��i z�kladn� m�n� (nap�. CZK)
    }
}
