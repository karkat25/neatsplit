using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NeatSplit.Models;
using NeatSplit.Services;

namespace neatsplit.ViewModels
{
    public class ItemParticipant
    {
        public int MemberId { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }

    public class ExpenseItemInput
    {
        public string Description { get; set; }
        public double Cost { get; set; }
        public ObservableCollection<ItemParticipant> Participants { get; set; } = new();
    }

    public class AddExpensePageViewModel : BaseViewModel
    {
        private readonly NeatSplitDatabase _database;
        private readonly int _groupId;
        public ObservableCollection<Member> Members { get; set; } = new();
        public ObservableCollection<ExpenseItemInput> Items { get; set; } = new();
        public string Description { get; set; }
        public string Total { get; set; }
        public Member SelectedPayer { get; set; }

        public AddExpensePageViewModel(NeatSplitDatabase database, int groupId)
        {
            _database = database;
            _groupId = groupId;
        }

        public async Task LoadMembersAsync()
        {
            Members.Clear();
            var members = await _database.GetMembersForGroupAsync(_groupId);
            foreach (var m in members)
                Members.Add(m);
        }

        public void AddItem()
        {
            var participants = Members.Select(m => new ItemParticipant { MemberId = m.Id, Name = m.Name, IsSelected = false }).ToList();
            Items.Add(new ExpenseItemInput { Participants = new ObservableCollection<ItemParticipant>(participants) });
        }

        public async Task<bool> SaveExpenseAsync()
        {
            if (string.IsNullOrWhiteSpace(Description) || SelectedPayer == null || !double.TryParse(Total, out double totalAmount) || Items.Count == 0)
                return false;

            var expense = new Expense
            {
                GroupId = _groupId,
                Description = Description,
                TotalAmount = totalAmount,
                PayerMemberId = SelectedPayer.Id
            };
            await _database.SaveExpenseAsync(expense);

            foreach (var item in Items)
            {
                var expenseItem = new ExpenseItem
                {
                    ExpenseId = expense.Id,
                    Description = item.Description,
                    Cost = item.Cost
                };
                await _database.SaveExpenseItemAsync(expenseItem);

                foreach (var participant in item.Participants.Where(p => p.IsSelected))
                {
                    var eip = new ExpenseItemParticipant
                    {
                        ExpenseItemId = expenseItem.Id,
                        MemberId = participant.MemberId
                    };
                    await _database.SaveExpenseItemParticipantAsync(eip);
                }
            }
            return true;
        }
    }
} 