using System.Collections.ObjectModel;
using NeatSplit.Models;

namespace NeatSplit;

public class PaidPayment
{
    public int FromMemberId { get; set; }
    public int ToMemberId { get; set; }
    public int GroupId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidDate { get; set; }
}

public static class AppData
{
    public static ObservableCollection<Group> Groups { get; } = new();
    public static ObservableCollection<Member> Members { get; } = new();
    public static ObservableCollection<Expense> Expenses { get; } = new();
    public static ObservableCollection<PaidPayment> PaidPayments { get; } = new();

    public static void NotifyGroupDataChanged(int groupId)
    {
        var group = Groups.FirstOrDefault(g => g.Id == groupId);
        group?.NotifyCalculatedPropertiesChanged();
    }

    public static void AddExpense(Expense expense)
    {
        Expenses.Add(expense);
        NotifyGroupDataChanged(expense.GroupId);
        
        // Clear paid payments for this group since payment instructions will change
        var paymentsToRemove = PaidPayments.Where(p => p.GroupId == expense.GroupId).ToList();
        foreach (var payment in paymentsToRemove)
        {
            PaidPayments.Remove(payment);
        }
    }

    public static void RemoveExpense(Expense expense)
    {
        var groupId = expense.GroupId;
        Expenses.Remove(expense);
        NotifyGroupDataChanged(groupId);
        
        // Clear paid payments for this group since payment instructions will change
        var paymentsToRemove = PaidPayments.Where(p => p.GroupId == groupId).ToList();
        foreach (var payment in paymentsToRemove)
        {
            PaidPayments.Remove(payment);
        }
    }

    public static void AddMember(Member member)
    {
        Members.Add(member);
        NotifyGroupDataChanged(member.GroupId);
    }

    public static void RemoveMember(Member member)
    {
        var groupId = member.GroupId;
        Members.Remove(member);
        NotifyGroupDataChanged(groupId);
    }
} 