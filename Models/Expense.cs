using SQLite;

namespace NeatSplit.Models;

public enum SplitType
{
    Equal,
    Custom,
    Percentage
}

[Table("Expense")]
public class Expense
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public int GroupId { get; set; }
    
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
    
    public decimal Amount { get; set; }
    
    public DateTime Date { get; set; } = DateTime.Now;
    
    public int PaidByMemberId { get; set; }
    
    public SplitType SplitType { get; set; } = SplitType.Equal;
    
    [Ignore]
    public Dictionary<int, decimal> CustomSplitAmounts { get; set; } = new(); // MemberId -> Amount
    
    [Ignore]
    public Dictionary<int, decimal> PercentageSplitAmounts { get; set; } = new(); // MemberId -> Percentage
    
    [Ignore]
    public List<int> Participants { get; set; } = new(); // MemberIds who participated
    
    [Ignore]
    public string PaidByMemberName 
    { 
        get 
        {
            var member = AppData.Members.FirstOrDefault(m => m.Id == PaidByMemberId);
            return member?.Name ?? "Unknown";
        }
    }
    
    public decimal GetMemberShare(int memberId)
    {
        var participants = AppData.Members.Where(m => Participants.Contains(m.Id)).ToList();
        var participantCount = participants.Count;
        
        if (participantCount == 0) return 0;
        
        return SplitType switch
        {
            SplitType.Equal => Amount / participantCount,
            SplitType.Custom => CustomSplitAmounts.TryGetValue(memberId, out var amount) ? amount : 0,
            SplitType.Percentage => PercentageSplitAmounts.TryGetValue(memberId, out var percentage) ? (Amount * percentage / 100) : 0,
            _ => Amount / participantCount
        };
    }
} 