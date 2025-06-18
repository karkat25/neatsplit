using SQLite;
using System;

namespace NeatSplit.Models
{
    /// <summary>
    /// Represents an overall expense incurred by a group.
    /// </summary>
    [Table("Expenses")]
    public class Expense
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public int GroupId { get; set; }

        [NotNull]
        public string Description { get; set; }

        [NotNull]
        public double TotalAmount { get; set; }

        [NotNull]
        public int PayerMemberId { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 