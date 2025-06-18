using SQLite;
using System;

namespace NeatSplit.Models
{
    /// <summary>
    /// Represents a group where expenses are split.
    /// </summary>
    [Table("Groups")]
    public class Group
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 