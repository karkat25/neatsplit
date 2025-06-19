using System.Collections.Generic;
using System.Linq;
using NeatSplit.Models;
using System.Threading.Tasks;

namespace NeatSplit.Core
{
    public class BalanceCalculator
    {
        public static List<BalanceResult> CalculateBalances(
            List<Member> members,
            List<Expense> expenses,
            List<ExpenseItem> expenseItems,
            List<ExpenseItemParticipant> itemParticipants)
        {
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

            var net = members.ToDictionary(m => m.Id, m => memberPaid[m.Id] - memberOwes[m.Id]);
            var results = new List<BalanceResult>();
            
            // Create mutable lists of creditors and debtors
            var creditors = net.Where(kv => kv.Value > 0.01)
                              .Select(kv => (memberId: kv.Key, amount: kv.Value))
                              .OrderByDescending(c => c.amount)
                              .ToList();
            var debtors = net.Where(kv => kv.Value < -0.01)
                            .Select(kv => (memberId: kv.Key, amount: -kv.Value))
                            .OrderByDescending(d => d.amount)
                            .ToList();

            // Process each debtor
            for (int di = 0; di < debtors.Count; di++)
            {
                var debtor = debtors[di];
                double remainingDebt = debtor.amount;

                // Find creditors to pay off this debt
                for (int ci = 0; ci < creditors.Count && remainingDebt > 0.01; ci++)
                {
                    var creditor = creditors[ci];
                    if (creditor.amount <= 0.01) continue;

                    double transferAmount = System.Math.Min(remainingDebt, creditor.amount);

                    // Add the balance result
                    string fromMember = members.First(m => m.Id == debtor.memberId).Name;
                    string toMember = members.First(m => m.Id == creditor.memberId).Name;
                    
                    results.Add(new BalanceResult 
                    { 
                        From = fromMember, 
                        To = toMember, 
                        Amount = transferAmount 
                    });

                    // Update remaining amounts
                    remainingDebt -= transferAmount;
                    creditors[ci] = (creditor.memberId, creditor.amount - transferAmount);
                }
            }

            return results;
        }

        /// <summary>
        /// Async method for calculating balances from expenses and members
        /// </summary>
        public async Task<List<BalanceResult>> CalculateBalancesAsync(
            List<Expense> expenses, 
            List<Member> members)
        {
            // For now, we'll use a simplified calculation
            // In a real implementation, you'd fetch expense items and participants from the database
            
            var memberPaid = members.ToDictionary(m => m.Id, m => 0.0);
            var memberOwes = members.ToDictionary(m => m.Id, m => 0.0);

            // Calculate what each member paid
            foreach (var expense in expenses)
            {
                if (memberPaid.ContainsKey(expense.PayerMemberId))
                    memberPaid[expense.PayerMemberId] += expense.TotalAmount;
            }

            // For now, assume equal split among all members
            double totalExpenses = expenses.Sum(e => e.TotalAmount);
            double perPersonShare = totalExpenses / members.Count;

            foreach (var member in members)
            {
                memberOwes[member.Id] = perPersonShare;
            }

            // Calculate net balances
            var net = members.ToDictionary(m => m.Id, m => memberPaid[m.Id] - memberOwes[m.Id]);
            
            // Create balance results
            var results = new List<BalanceResult>();
            
            var creditors = net.Where(kv => kv.Value > 0.01)
                              .Select(kv => (memberId: kv.Key, amount: kv.Value))
                              .OrderByDescending(c => c.amount)
                              .ToList();
            var debtors = net.Where(kv => kv.Value < -0.01)
                            .Select(kv => (memberId: kv.Key, amount: -kv.Value))
                            .OrderByDescending(d => d.amount)
                            .ToList();

            // Process each debtor
            for (int di = 0; di < debtors.Count; di++)
            {
                var debtor = debtors[di];
                double remainingDebt = debtor.amount;

                // Find creditors to pay off this debt
                for (int ci = 0; ci < creditors.Count && remainingDebt > 0.01; ci++)
                {
                    var creditor = creditors[ci];
                    if (creditor.amount <= 0.01) continue;

                    double transferAmount = System.Math.Min(remainingDebt, creditor.amount);

                    // Add the balance result
                    string fromMember = members.First(m => m.Id == debtor.memberId).Name;
                    string toMember = members.First(m => m.Id == creditor.memberId).Name;
                    
                    results.Add(new BalanceResult 
                    { 
                        From = fromMember, 
                        To = toMember, 
                        Amount = transferAmount 
                    });

                    // Update remaining amounts
                    remainingDebt -= transferAmount;
                    creditors[ci] = (creditor.memberId, creditor.amount - transferAmount);
                }
            }

            return results;
        }
    }
} 