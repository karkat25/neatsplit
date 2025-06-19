using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeatSplit.Models;
using NeatSplit.Services;
using NeatSplit.Core;

namespace NeatSplit.ViewModels
{
    public class BalanceDisplay
    {
        public string Display { get; set; }
    }

    public class BalancesTabViewModel : BaseViewModel
    {
        private readonly NeatSplitDatabase _database;
        private readonly int _groupId;
        public ObservableCollection<BalanceResult> Balances { get; set; } = new();

        public BalancesTabViewModel(NeatSplitDatabase database, int groupId)
        {
            _database = database;
            _groupId = groupId;
        }

        public async Task LoadBalancesAsync()
        {
            Balances.Clear();
            
            // Get all expenses for the group
            var expenses = await _database.GetExpensesForGroupAsync(_groupId);
            var members = await _database.GetMembersForGroupAsync(_groupId);
            
            if (expenses.Any() && members.Any())
            {
                var calculator = new BalanceCalculator();
                var balances = await calculator.CalculateBalancesAsync(expenses, members);
                
                foreach (var balance in balances)
                {
                    Balances.Add(balance);
                }
            }
        }

        public void LoadBalances()
        {
            _ = LoadBalancesAsync();
        }

        public async Task LoadBalancesFromGroupAsync()
        {
            Balances.Clear();
            
            try
            {
                var group = await _database.GetGroupAsync(_groupId);
                if (group == null)
                {
                    Balances.Add(new BalanceDisplay { Display = "Group not found" });
                    return;
                }

                group.Members = await _database.GetMembersForGroupAsync(_groupId);
                group.Expenses = await _database.GetExpensesForGroupAsync(_groupId);
                
                foreach (var expense in group.Expenses)
                {
                    expense.Items = await _database.GetExpenseItemsForExpenseAsync(expense.Id);
                    foreach (var item in expense.Items)
                    {
                        item.Participants = await _database.GetParticipantsForExpenseItemAsync(item.Id);
                    }
                }

                var balanceResults = BalanceCalculator.CalculateBalances(group);

                if (balanceResults.Count == 0)
                {
                    Balances.Add(new BalanceDisplay { Display = "All settled up!" });
                }
                else
                {
                    foreach (var result in balanceResults)
                    {
                        Balances.Add(new BalanceDisplay 
                        { 
                            Display = $"{result.From} owes {result.To} ${result.Amount:F2}" 
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Balances.Add(new BalanceDisplay { Display = $"Error calculating balances: {ex.Message}" });
            }
        }
    }
} 