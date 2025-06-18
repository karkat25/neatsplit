using System.Collections.Generic;
using System.Linq;

namespace NeatSplit.Core
{
    public class Member
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Expense
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string Description { get; set; }
        public double TotalAmount { get; set; }
        public int PayerMemberId { get; set; }
    }
    public class ExpenseItem
    {
        public int Id { get; set; }
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public double Cost { get; set; }
    }
    public class ExpenseItemParticipant
    {
        public int ExpenseItemId { get; set; }
        public int MemberId { get; set; }
    }
    public class BalanceResult
    {
        public string From { get; set; }
        public string To { get; set; }
        public double Amount { get; set; }
    }
    public static class BalanceCalculator
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
            var creditors = net.Where(kv => kv.Value > 0).OrderByDescending(kv => kv.Value).ToList();
            var debtors = net.Where(kv => kv.Value < 0).OrderBy(kv => kv.Value).ToList();
            var results = new List<BalanceResult>();
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
                    results.Add(new BalanceResult { From = from, To = to, Amount = amount });
                    net[creditor.Key] -= amount;
                    net[debtor.Key] += amount;
                }
                if (net[creditor.Key] < 0.01) ci++;
                if (net[debtor.Key] > -0.01) di++;
            }
            return results;
        }
    }
} 