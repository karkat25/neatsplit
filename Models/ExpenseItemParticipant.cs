using SQLite;
using System;

namespace NeatSplit.Models
{
    /// <summary>
    /// Junction table to link ExpenseItems to Members who participated in them.
    /// </summary>
    [Table("ExpenseItemParticipants")]
    public class ExpenseItemParticipant
    {
        [Indexed]
        public int ExpenseItemId { get; set; }

        [Indexed]
        public int MemberId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 