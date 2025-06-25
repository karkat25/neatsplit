using NeatSplit.Models;
using System.Collections.ObjectModel;

namespace NeatSplit.Views;

public class MemberBalance
{
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public partial class GroupBalancesPage : ContentPage
{
    public ObservableCollection<MemberBalance> Balances { get; set; }

    public GroupBalancesPage(Group group)
    {
        InitializeComponent();
        Title = $"Balances for {group.Name}";
        Balances = new ObservableCollection<MemberBalance>(CalculateBalances(group));
        BindingContext = this;
    }

    private List<MemberBalance> CalculateBalances(Group group)
    {
        var members = AppData.Members.Where(m => m.GroupId == group.Id).ToList();
        var expenses = AppData.Expenses.Where(e => e.GroupId == group.Id).ToList();
        if (members.Count == 0 || expenses.Count == 0)
            return members.Select(m => new MemberBalance { Name = m.Name, Balance = 0 }).ToList();

        decimal total = expenses.Sum(e => e.Amount);
        decimal share = total / members.Count;
        var paidBy = members.ToDictionary(m => m.Id, m => expenses.Where(e => e.PaidByMemberId == m.Id).Sum(e => e.Amount));

        return members.Select(m => new MemberBalance
        {
            Name = m.Name,
            Balance = paidBy[m.Id] - share
        }).ToList();
    }
} 