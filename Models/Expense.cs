namespace NeatSplit.Models;

public class Expense
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public int PaidByMemberId { get; set; }
} 