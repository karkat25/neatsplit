namespace NeatSplit.Models;

public enum SplitType
{
    Equal,
    Custom,
    Percentage
}

public class Expense
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public int PaidByMemberId { get; set; }
    public SplitType SplitType { get; set; } = SplitType.Equal;
    public Dictionary<int, decimal> CustomSplitAmounts { get; set; } = new(); // MemberId -> Amount
    public Dictionary<int, decimal> PercentageSplitAmounts { get; set; } = new(); // MemberId -> Percentage
    public List<int> Participants { get; set; } = new(); // MemberIds who participated
    
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