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
            System.Diagnostics.Debug.WriteLine("AppData.InitializeAsync started");
            await LoadAllDataAsync();
            System.Diagnostics.Debug.WriteLine($"AppData.InitializeAsync completed. Groups: {Groups.Count}, Members: {Members.Count}, Expenses: {Expenses.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AppData.InitializeAsync error: {ex.Message}");
            throw;
        }
    }
    
    public static async Task LoadAllDataAsync()
    {
        System.Diagnostics.Debug.WriteLine("LoadAllDataAsync started");
        
        var groups = await DatabaseService.GetGroupsAsync();
        System.Diagnostics.Debug.WriteLine($"Loaded {groups.Count} groups from database:");
        foreach (var group in groups)
        {
            System.Diagnostics.Debug.WriteLine($"  - Group: '{group.Name}' (ID: {group.Id}, Description: '{group.Description}', Created: {group.CreatedDate})");
        }
        
        var members = await DatabaseService.GetMembersAsync();
        var expenses = await DatabaseService.GetExpensesAsync();
        var paidPayments = await DatabaseService.GetPaidPaymentsAsync();
        
        Groups.Clear();
        Members.Clear();
        Expenses.Clear();
        PaidPayments.Clear();
        
        System.Diagnostics.Debug.WriteLine("Adding groups to ObservableCollection:");
        foreach (var group in groups)
        {
            Groups.Add(group);
            System.Diagnostics.Debug.WriteLine($"  - Added to UI: '{group.Name}' (ID: {group.Id})");
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
        
        System.Diagnostics.Debug.WriteLine($"LoadAllDataAsync completed. Groups: {Groups.Count}, Members: {Members.Count}, Expenses: {Expenses.Count}");
        
        // Refresh calculated properties for all groups after loading data
        RefreshAllGroups();
        System.Diagnostics.Debug.WriteLine("Refreshed calculated properties for all groups");
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
            System.Diagnostics.Debug.WriteLine($"Adding expense: {expense.Description} for ${expense.Amount}");
            await DatabaseService.SaveExpenseAsync(expense);
            System.Diagnostics.Debug.WriteLine($"Expense saved to database with ID: {expense.Id}");
            Expenses.Add(expense);
            RefreshAllGroups();
            
            // Clear paid payments for this group since payment instructions will change
            await ClearPaidPaymentsForGroupAsync(expense.GroupId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding expense: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Adding member: {member.Name}");
            await DatabaseService.SaveMemberAsync(member);
            System.Diagnostics.Debug.WriteLine($"Member saved to database with ID: {member.Id}");
            Members.Add(member);
            RefreshAllGroups();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding member: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"AddGroupAsync called for group: '{group.Name}' (ID: {group.Id}, Description: '{group.Description}', Created: {group.CreatedDate})");
            System.Diagnostics.Debug.WriteLine($"About to save group to database...");
            await DatabaseService.SaveGroupAsync(group);
            System.Diagnostics.Debug.WriteLine($"Group saved to database with ID: {group.Id}");
            System.Diagnostics.Debug.WriteLine($"About to add group to UI collection...");
            Groups.Add(group);
            System.Diagnostics.Debug.WriteLine($"Group added to UI collection. Total groups in UI: {Groups.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding group: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public static async Task DeleteGroupAsync(Group group)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Deleting group: {group.Name}");
            await DatabaseService.DeleteGroupAsync(group);
            System.Diagnostics.Debug.WriteLine($"Group deleted from database");
            Groups.Remove(group);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting group: {ex.Message}");
            throw;
        }
    }

    public static async Task AddPaidPaymentAsync(PaidPayment payment)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Adding paid payment: {payment.FromMemberId} -> {payment.ToMemberId} for ${payment.Amount}");
            await DatabaseService.SavePaidPaymentAsync(payment);
            System.Diagnostics.Debug.WriteLine($"Paid payment saved to database with ID: {payment.Id}");
            PaidPayments.Add(payment);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding paid payment: {ex.Message}");
            throw;
        }
    }

    public static async Task RemovePaidPaymentAsync(PaidPayment payment)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Removing paid payment: {payment.FromMemberId} -> {payment.ToMemberId} for ${payment.Amount}");
            await DatabaseService.DeletePaidPaymentAsync(payment);
            System.Diagnostics.Debug.WriteLine($"Paid payment removed from database");
            PaidPayments.Remove(payment);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error removing paid payment: {ex.Message}");
            throw;
        }
    }

    public static async Task ClearAllDataAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Clearing all data from database...");
            
            // Clear all tables
            await DatabaseService.ClearAllDataAsync();
            
            // Clear UI collections
            Groups.Clear();
            Members.Clear();
            Expenses.Clear();
            PaidPayments.Clear();
            
            System.Diagnostics.Debug.WriteLine("All data cleared successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing data: {ex.Message}");
            throw;
        }
    }
} 