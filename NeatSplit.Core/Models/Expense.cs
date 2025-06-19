namespace NeatSplit.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string Description { get; set; }
        public double TotalAmount { get; set; }
        public int PayerMemberId { get; set; }
        public DateTime ExpenseDate { get; set; }
    }
} 