using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NeatSplit.Models;
using NeatSplit.Services;

namespace NeatSplit.ViewModels
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
        
        public ObservableCollection<ExpenseItem> Items { get; set; } = new();
        public ObservableCollection<Member> Members { get; set; } = new();
        
        public string Description { get; set; }
        public double TotalAmount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public Member SelectedPayer { get; set; }

        public AddExpensePageViewModel(NeatSplitDatabase database, int groupId)
        {
            _database = database;
            _groupId = groupId;
            LoadMembers();
        }

        private async void LoadMembers()
        {
            var members = await _database.GetMembersForGroupAsync(_groupId);
            Members.Clear();
            foreach (var member in members)
            {
                Members.Add(member);
            }
        }

        public void AddItem(ExpenseItem item)
        {
            Items.Add(item);
        }

        public void UpdateItem(ExpenseItem item)
        {
            // The item is already in the collection, so we just need to trigger UI update
            OnPropertyChanged(nameof(Items));
        }

        public void RemoveItem(ExpenseItem item)
        {
            Items.Remove(item);
        }

        public async Task SaveExpenseAsync()
        {
            if (string.IsNullOrWhiteSpace(Description) || TotalAmount <= 0 || SelectedPayer == null)
            {
                throw new InvalidOperationException("Please fill all required fields");
            }

            var expense = new Expense
            {
                Id = 0, // Always reset for new
                Description = Description,
                TotalAmount = TotalAmount,
                Date = Date,
                GroupId = _groupId,
                PayerMemberId = SelectedPayer.Id,
                CreatedDate = DateTime.Now
            };

            await _database.AddExpenseAsync(expense);

            // Add expense items
            foreach (var item in Items)
            {
                item.Id = 0; // Always reset for new
                item.ExpenseId = expense.Id;
                item.CreatedDate = DateTime.Now;
                await _database.AddExpenseItemAsync(item);
            }
        }
    }
} 