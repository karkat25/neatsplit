using SQLite;
using System;

namespace NeatSplit.Models
{
    /// <summary>
    /// Represents an individual item within an expense bill.
    /// </summary>
    [Table("ExpenseItems")]
    public class ExpenseItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public int ExpenseId { get; set; }

        [NotNull]
        public string Description { get; set; }

        [NotNull]
        public double Cost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 