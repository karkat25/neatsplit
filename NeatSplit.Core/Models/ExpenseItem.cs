namespace NeatSplit.Models
{
    public class ExpenseItem
    {
        public int Id { get; set; }
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public double Cost { get; set; }
    }
} 