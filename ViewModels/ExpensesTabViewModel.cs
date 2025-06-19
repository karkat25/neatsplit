using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using NeatSplit.Models;
using NeatSplit.Services;

namespace NeatSplit.ViewModels
{
    public class ExpenseDisplay
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double TotalAmount { get; set; }
        public string PayerName { get; set; }
    }

    public class ExpensesTabViewModel : BaseViewModel
    {
        private readonly NeatSplitDatabase _database;
        private readonly int _groupId;
        public ObservableCollection<Expense> Expenses { get; set; } = new();

        public ExpensesTabViewModel(NeatSplitDatabase database, int groupId)
        {
            _database = database;
            _groupId = groupId;
        }

        public async Task LoadExpensesAsync()
        {
            Expenses.Clear();
            var expenses = await _database.GetExpensesForGroupAsync(_groupId);
            foreach (var expense in expenses)
            {
                Expenses.Add(expense);
            }
        }

        public void LoadExpenses()
        {
            _ = LoadExpensesAsync();
        }
    }
} 