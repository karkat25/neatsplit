using SQLite;

namespace NeatSplit.Models;

[Table("Member")]
public class Member
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public int GroupId { get; set; } // The group this member belongs to
} 