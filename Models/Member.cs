using SQLite;
using System;

namespace NeatSplit.Models
{
    /// <summary>
    /// Represents a member within a specific group.
    /// </summary>
    [Table("Members")]
    public class Member
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public int GroupId { get; set; }

        [NotNull]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 