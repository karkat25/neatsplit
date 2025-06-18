using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeatSplit.Models;
using NeatSplit.Services;

namespace neatsplit.ViewModels
{
    public class BalanceDisplay
    {
        public string Display { get; set; }
    }

    public class BalancesTabViewModel : BaseViewModel
    {
        private readonly NeatSplitDatabase _database;
        private int _groupId;
        public ObservableCollection<BalanceDisplay> Balances { get; set; } = new();

        public BalancesTabViewModel(NeatSplitDatabase database, int groupId)
        {
            _database = database;
            _groupId = groupId;
        }

        public async Task LoadBalancesAsync()
        {
            Balances.Clear();
            var members = await _database.GetMembersForGroupAsync(_groupId);
            var expenses = await _database.GetExpensesForGroupAsync(_groupId);
            var expenseItems = new List<ExpenseItem>();
            var itemParticipants = new List<ExpenseItemParticipant>();
            foreach (var expense in expenses)
            {
                var items = await _database.GetExpenseItemsForExpenseAsync(expense.Id);
                expenseItems.AddRange(items);
                foreach (var item in items)
                {
                    var participants = await _database.GetParticipantsForExpenseItemAsync(item.Id);
                    itemParticipants.AddRange(participants);
                }
            }

            // Calculate how much each member paid and owes
            var memberPaid = members.ToDictionary(m => m.Id, m => 0.0);
            var memberOwes = members.ToDictionary(m => m.Id, m => 0.0);

            foreach (var expense in expenses)
            {
                if (memberPaid.ContainsKey(expense.PayerMemberId))
                    memberPaid[expense.PayerMemberId] += expense.TotalAmount;
            }

            foreach (var item in expenseItems)
            {
                var participants = itemParticipants.Where(p => p.ExpenseItemId == item.Id).ToList();
                if (participants.Count == 0) continue;
                double share = item.Cost / participants.Count;
                foreach (var p in participants)
                {
                    if (memberOwes.ContainsKey(p.MemberId))
                        memberOwes[p.MemberId] += share;
                }
            }

            // Calculate net balances
            var net = members.ToDictionary(m => m.Id, m => memberPaid[m.Id] - memberOwes[m.Id]);

            // Calculate who owes whom (simplified, not optimal for minimal transactions)
            var creditors = net.Where(kv => kv.Value > 0).OrderByDescending(kv => kv.Value).ToList();
            var debtors = net.Where(kv => kv.Value < 0).OrderBy(kv => kv.Value).ToList();
            var results = new List<string>();
            int ci = 0, di = 0;
            while (ci < creditors.Count && di < debtors.Count)
            {
                var creditor = creditors[ci];
                var debtor = debtors[di];
                double amount = System.Math.Min(creditor.Value, -debtor.Value);
                if (amount > 0.01)
                {
                    string from = members.First(m => m.Id == debtor.Key).Name;
                    string to = members.First(m => m.Id == creditor.Key).Name;
                    results.Add($"{from} owes {to} ${amount:F2}");
                    net[creditor.Key] -= amount;
                    net[debtor.Key] += amount;
                }
                if (net[creditor.Key] < 0.01) ci++;
                if (net[debtor.Key] > -0.01) di++;
            }
            if (results.Count == 0)
                results.Add("All settled up!");
            foreach (var r in results)
                Balances.Add(new BalanceDisplay { Display = r });
        }
    }
} 