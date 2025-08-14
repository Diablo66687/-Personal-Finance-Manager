public class Goal
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime? Deadline { get; set; }
    public List<int> SharedUserIds { get; set; } = new(); // ID uivatelù, kteøí mají pøístup
}
