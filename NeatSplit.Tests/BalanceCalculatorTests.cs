using Xunit;
using NeatSplit.Core;
using System.Collections.Generic;

namespace NeatSplit.Tests
{
    public class BalanceCalculatorTests
    {
        [Fact]
        public void SimpleTwoPersonSplit_ShouldCalculateCorrectly()
        {
            var members = new List<Member>
            {
                new Member { Id = 1, Name = "Alice" },
                new Member { Id = 2, Name = "Bob" }
            };
            var expenses = new List<Expense>
            {
                new Expense { Id = 1, GroupId = 1, Description = "Lunch", TotalAmount = 20, PayerMemberId = 1 }
            };
            var items = new List<ExpenseItem>
            {
                new ExpenseItem { Id = 1, ExpenseId = 1, Description = "Lunch", Cost = 20 }
            };
            var participants = new List<ExpenseItemParticipant>
            {
                new ExpenseItemParticipant { ExpenseItemId = 1, MemberId = 1 },
                new ExpenseItemParticipant { ExpenseItemId = 1, MemberId = 2 }
            };

            var results = BalanceCalculator.CalculateBalances(members, expenses, items, participants);

            Assert.Single(results);
            Assert.Equal("Bob", results[0].From);
            Assert.Equal("Alice", results[0].To);
            Assert.Equal(10, results[0].Amount, 2);
        }

        [Fact]
        public void ThreePersonSplit_ShouldCalculateCorrectly()
        {
            var members = new List<Member>
            {
                new Member { Id = 1, Name = "Alice" },
                new Member { Id = 2, Name = "Bob" },
                new Member { Id = 3, Name = "Charlie" }
            };
            var expenses = new List<Expense>
            {
                new Expense { Id = 1, GroupId = 1, Description = "Dinner", TotalAmount = 90, PayerMemberId = 1 }
            };
            var items = new List<ExpenseItem>
            {
                new ExpenseItem { Id = 1, ExpenseId = 1, Description = "Dinner", Cost = 90 }
            };
            var participants = new List<ExpenseItemParticipant>
            {
                new ExpenseItemParticipant { ExpenseItemId = 1, MemberId = 1 },
                new ExpenseItemParticipant { ExpenseItemId = 1, MemberId = 2 },
                new ExpenseItemParticipant { ExpenseItemId = 1, MemberId = 3 }
            };
            var results = BalanceCalculator.CalculateBalances(members, expenses, items, participants);
            Assert.Equal(2, results.Count);
            Assert.Contains(results, r => r.From == "Bob" && r.To == "Alice" && r.Amount == 30);
            Assert.Contains(results, r => r.From == "Charlie" && r.To == "Alice" && r.Amount == 30);
        }

        [Fact]
        public void UnevenItemization_ShouldCalculateCorrectly()
        {
            var members = new List<Member>
            {
                new Member { Id = 1, Name = "Alice" },
                new Member { Id = 2, Name = "Bob" },
                new Member { Id = 3, Name = "Charlie" }
            };
            var expenses = new List<Expense>
            {
                new Expense { Id = 1, GroupId = 1, Description = "Pizza", TotalAmount = 30, PayerMemberId = 2 }
            };
            var items = new List<ExpenseItem>
            {
                new ExpenseItem { Id = 1, ExpenseId = 1, Description = "Veg", Cost = 10 },
                new ExpenseItem { Id = 2, ExpenseId = 1, Description = "NonVeg", Cost = 20 }
            };
            var participants = new List<ExpenseItemParticipant>
            {
                new ExpenseItemParticipant { ExpenseItemId = 1, MemberId = 1 },
                new ExpenseItemParticipant { ExpenseItemId = 1, MemberId = 2 },
                new ExpenseItemParticipant { ExpenseItemId = 2, MemberId = 2 },
                new ExpenseItemParticipant { ExpenseItemId = 2, MemberId = 3 }
            };
            var results = BalanceCalculator.CalculateBalances(members, expenses, items, participants);
            Assert.Equal(2, results.Count);
            Assert.Contains(results, r => r.From == "Alice" && r.To == "Bob" && r.Amount == 5);
            Assert.Contains(results, r => r.From == "Charlie" && r.To == "Bob" && r.Amount == 10);
        }

        [Fact]
        public void AllSettledUp_ShouldReturnEmpty()
        {
            var members = new List<Member>
            {
                new Member { Id = 1, Name = "Alice" },
                new Member { Id = 2, Name = "Bob" }
            };
            var expenses = new List<Expense>
            {
                new Expense { Id = 1, GroupId = 1, Description = "Lunch", TotalAmount = 10, PayerMemberId = 1 },
                new Expense { Id = 2, GroupId = 1, Description = "Dinner", TotalAmount = 10, PayerMemberId = 2 }
            };
            var items = new List<ExpenseItem>
            {
                new ExpenseItem { Id = 1, ExpenseId = 1, Description = "Lunch", Cost = 10 },
                new ExpenseItem { Id = 2, ExpenseId = 2, Description = "Dinner", Cost = 10 }
            };
            var participants = new List<ExpenseItemParticipant>
            {
                new ExpenseItemParticipant { ExpenseItemId = 1, MemberId = 1 },
                new ExpenseItemParticipant { ExpenseItemId = 1, MemberId = 2 },
                new ExpenseItemParticipant { ExpenseItemId = 2, MemberId = 1 },
                new ExpenseItemParticipant { ExpenseItemId = 2, MemberId = 2 }
            };
            var results = BalanceCalculator.CalculateBalances(members, expenses, items, participants);
            Assert.Empty(results);
        }

        [Fact]
        public void OnePersonPaysAll_ShouldCalculateCorrectly()
        {
            var members = new List<Member>
            {
                new Member { Id = 1, Name = "Alice" },
                new Member { Id = 2, Name = "Bob" },
                new Member { Id = 3, Name = "Charlie" }
            };
            var expenses = new List<Expense>
            {
                new Expense { Id = 1, GroupId = 1, Description = "Hotel", TotalAmount = 300, PayerMemberId = 3 }
            };
            var items = new List<ExpenseItem>
            {
                new ExpenseItem { Id = 1, ExpenseId = 1, Description = "Hotel", Cost = 300 }
            };
            var participants = new List<ExpenseItemParticipant>
            {
                new ExpenseItemParticipant { ExpenseItemId = 1, MemberId = 1 },
                new ExpenseItemParticipant { ExpenseItemId = 1, MemberId = 2 },
                new ExpenseItemParticipant { ExpenseItemId = 1, MemberId = 3 }
            };
            var results = BalanceCalculator.CalculateBalances(members, expenses, items, participants);
            Assert.Equal(2, results.Count);
            Assert.Contains(results, r => r.From == "Alice" && r.To == "Charlie" && r.Amount == 100);
            Assert.Contains(results, r => r.From == "Bob" && r.To == "Charlie" && r.Amount == 100);
        }

        [Fact]
        public void NoExpenses_ShouldReturnEmpty()
        {
            var members = new List<Member>
            {
                new Member { Id = 1, Name = "Alice" },
                new Member { Id = 2, Name = "Bob" }
            };
            var expenses = new List<Expense>();
            var items = new List<ExpenseItem>();
            var participants = new List<ExpenseItemParticipant>();
            var results = BalanceCalculator.CalculateBalances(members, expenses, items, participants);
            Assert.Empty(results);
        }
    }
} 