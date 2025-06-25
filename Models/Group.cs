namespace NeatSplit.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public decimal TotalExpenses =>
        NeatSplit.AppData.Expenses.Where(e => e.GroupId == Id).Sum(e => e.Amount);
} 