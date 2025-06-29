using System.Collections.ObjectModel;
using NeatSplit.Models;
using NeatSplit.Services;
using SQLite;

namespace NeatSplit;

[Table("PaidPayment")]
public class PaidPayment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public int FromMemberId { get; set; }
    public int ToMemberId { get; set; }
    public int GroupId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidDate { get; set; }
}

public static class AppData
{
    private static DatabaseService? _databaseService;
    
    private static DatabaseService DatabaseService
    {
        get
        {
            if (_databaseService == null)
            {
                _databaseService = new DatabaseService();
            }
            return _databaseService;
        }
    }
    
    public static ObservableCollection<Group> Groups { get; } = new();
    public static ObservableCollection<Member> Members { get; } = new();
    public static ObservableCollection<Expense> Expenses { get; } = new();
    public static ObservableCollection<PaidPayment> PaidPayments { get; } = new();
    
    public static async Task InitializeAsync()
    {
        try
        {
            await LoadAllDataAsync();
        }
        catch
        {
            throw;
        }
    }
    
    public static async Task LoadAllDataAsync()
    {
        var groups = await DatabaseService.GetGroupsAsync();
        var members = await DatabaseService.GetMembersAsync();
        var expenses = await DatabaseService.GetExpensesAsync();
        var paidPayments = await DatabaseService.GetPaidPaymentsAsync();
        
        Groups.Clear();
        Members.Clear();
        Expenses.Clear();
        PaidPayments.Clear();
        
        foreach (var group in groups)
        {
            Groups.Add(group);
        }
        
        foreach (var member in members)
        {
            Members.Add(member);
        }
        
        foreach (var expense in expenses)
        {
            Expenses.Add(expense);
        }
        
        foreach (var payment in paidPayments)
        {
            PaidPayments.Add(payment);
        }
        
        // Refresh calculated properties for all groups after loading data
        RefreshAllGroups();
    }

    public static void NotifyGroupDataChanged(int groupId)
    {
        var group = Groups.FirstOrDefault(g => g.Id == groupId);
        group?.NotifyCalculatedPropertiesChanged();
    }

    public static void RefreshAllGroups()
    {
        foreach (var group in Groups)
        {
            group.NotifyCalculatedPropertiesChanged();
        }
    }

    public static async Task AddExpense(Expense expense)
    {
        try
        {
            await DatabaseService.SaveExpenseAsync(expense);
            Expenses.Add(expense);
            RefreshAllGroups();
            
            // Clear paid payments for this group since payment instructions will change
            await ClearPaidPaymentsForGroupAsync(expense.GroupId);
        }
        catch
        {
            throw;
        }
    }

    public static async Task RemoveExpense(Expense expense)
    {
        await DatabaseService.DeleteExpenseAsync(expense);
        var groupId = expense.GroupId;
        Expenses.Remove(expense);
        RefreshAllGroups();
        
        // Clear paid payments for this group since payment instructions will change
        await ClearPaidPaymentsForGroupAsync(groupId);
    }

    public static async Task AddMember(Member member)
    {
        try
        {
            await DatabaseService.SaveMemberAsync(member);
            Members.Add(member);
            RefreshAllGroups();
        }
        catch
        {
            throw;
        }
    }

    public static async Task RemoveMember(Member member)
    {
        await DatabaseService.DeleteMemberAsync(member);
        var groupId = member.GroupId;
        Members.Remove(member);
        RefreshAllGroups();
    }
    
    public static async Task ClearPaidPaymentsForGroupAsync(int groupId)
    {
        await DatabaseService.ClearPaidPaymentsForGroupAsync(groupId);
        
        var paymentsToRemove = PaidPayments.Where(p => p.GroupId == groupId).ToList();
        foreach (var payment in paymentsToRemove)
        {
            PaidPayments.Remove(payment);
        }
    }

    public static async Task AddGroupAsync(Group group)
    {
        try
        {
            await DatabaseService.SaveGroupAsync(group);
            Groups.Add(group);
        }
        catch
        {
            throw;
        }
    }

    public static async Task DeleteGroupAsync(Group group)
    {
        try
        {
            await DatabaseService.DeleteGroupAsync(group);
            Groups.Remove(group);
        }
        catch
        {
            throw;
        }
    }

    public static async Task AddPaidPaymentAsync(PaidPayment payment)
    {
        try
        {
            await DatabaseService.SavePaidPaymentAsync(payment);
            PaidPayments.Add(payment);
        }
        catch
        {
            throw;
        }
    }

    public static async Task RemovePaidPaymentAsync(PaidPayment payment)
    {
        try
        {
            await DatabaseService.DeletePaidPaymentAsync(payment);
            PaidPayments.Remove(payment);
        }
        catch
        {
            throw;
        }
    }

    public static async Task ClearAllDataAsync()
    {
        try
        {
            await DatabaseService.ClearAllDataAsync();
            Groups.Clear();
            Members.Clear();
            Expenses.Clear();
            PaidPayments.Clear();
        }
        catch
        {
            throw;
        }
    }
} 