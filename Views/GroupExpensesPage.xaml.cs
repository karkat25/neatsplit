using NeatSplit.Models;
using System.Collections.ObjectModel;

namespace NeatSplit.Views;

public partial class GroupExpensesPage : ContentPage
{
    public ObservableCollection<Expense> Expenses { get; set; }
    private int _nextId = 1;
    private readonly Group _group;

    public GroupExpensesPage(Group group)
    {
        InitializeComponent();
        _group = group;
        Title = $"Expenses of {group.Name}";
        Expenses = new ObservableCollection<Expense>(AppData.Expenses.Where(e => e.GroupId == group.Id));
        BindingContext = this;
        if (AppData.Expenses.Any())
            _nextId = AppData.Expenses.Max(e => e.Id) + 1;
    }

    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        var description = await DisplayPromptAsync("Add Expense", "Enter expense description:");
        if (!string.IsNullOrWhiteSpace(description))
        {
            var amountText = await DisplayPromptAsync("Amount", "Enter expense amount (e.g., 25.50):");
            if (decimal.TryParse(amountText, out decimal amount))
            {
                // Pick payer
                var groupMembers = AppData.Members.Where(m => m.GroupId == _group.Id).ToList();
                if (groupMembers.Count == 0)
                {
                    await DisplayAlert("No Members", "Please add members to this group first.", "OK");
                    return;
                }
                string payer = await DisplayActionSheet("Who paid?", "Cancel", null, groupMembers.Select(m => m.Name).ToArray());
                if (payer == null || payer == "Cancel") return;
                var payerMember = groupMembers.First(m => m.Name == payer);

                var newExpense = new Expense
                {
                    Id = _nextId++,
                    GroupId = _group.Id,
                    Description = description.Trim(),
                    Amount = amount,
                    Date = DateTime.Now,
                    PaidByMemberId = payerMember.Id
                };
                
                AppData.Expenses.Add(newExpense);
                Expenses.Add(newExpense);
                await DisplayAlert("Success", $"Expense '{description}' added!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Please enter a valid amount.", "OK");
            }
        }
    }

    private async void OnDeleteExpenseClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int expenseId)
        {
            var expense = Expenses.FirstOrDefault(ex => ex.Id == expenseId);
            if (expense != null)
            {
                bool confirm = await DisplayAlert("Delete Expense", 
                    $"Are you sure you want to delete '{expense.Description}'?", "Yes", "No");
                
                if (confirm)
                {
                    AppData.Expenses.Remove(expense);
                    Expenses.Remove(expense);
                    await DisplayAlert("Deleted", $"Expense '{expense.Description}' has been deleted.", "OK");
                }
            }
        }
    }

    private async void OnEditExpenseClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int expenseId)
        {
            var expense = Expenses.FirstOrDefault(ex => ex.Id == expenseId);
            if (expense != null)
            {
                var newDescription = await DisplayPromptAsync("Edit Expense", "Edit description:", initialValue: expense.Description);
                if (string.IsNullOrWhiteSpace(newDescription)) return;
                var newAmountText = await DisplayPromptAsync("Edit Amount", "Edit amount:", initialValue: expense.Amount.ToString());
                if (!decimal.TryParse(newAmountText, out decimal newAmount))
                {
                    await DisplayAlert("Error", "Please enter a valid amount.", "OK");
                    return;
                }
                expense.Description = newDescription.Trim();
                expense.Amount = newAmount;
                // Refresh the list
                var idx = Expenses.IndexOf(expense);
                Expenses.RemoveAt(idx);
                Expenses.Insert(idx, expense);
                await DisplayAlert("Success", "Expense updated!", "OK");
            }
        }
    }
} 