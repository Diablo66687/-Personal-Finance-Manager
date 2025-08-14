namespace ConsoleApp4
{
    public class Budget
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Category { get; set; } // novì
        public decimal Limit { get; set; }
        public List<int> SharedUserIds { get; set; } = new(); // ID uivatelù, kteøí mají pøístup
    }
}