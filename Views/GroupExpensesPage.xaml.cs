using NeatSplit.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace NeatSplit.Views;

[QueryProperty(nameof(Group), "group")]
public partial class GroupExpensesPage : ContentPage
{
    public ObservableCollection<Expense> Expenses { get; set; }
    private int _nextId = 1;
    private Group _group = null!;

    public Group Group
    {
        get => _group;
        set
        {
            _group = value;
            Title = $"Expenses - {_group.Name}";
            Expenses = new ObservableCollection<Expense>(AppData.Expenses.Where(e => e.GroupId == _group.Id));
            BindingContext = this;
            if (AppData.Expenses.Any())
                _nextId = AppData.Expenses.Max(e => e.Id) + 1;
            
            UpdateAddButtonState();
        }
    }

    public GroupExpensesPage()
    {
        InitializeComponent();
        Expenses = new ObservableCollection<Expense>();
    }

    private void UpdateAddButtonState()
    {
        var groupMembers = AppData.Members.Where(m => m.GroupId == _group.Id).ToList();
        AddExpenseButton.IsEnabled = groupMembers.Count > 0;
    }

    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        var groupMembers = AppData.Members.Where(m => m.GroupId == _group.Id).ToList();
        if (groupMembers.Count == 0)
        {
            await DisplayAlert("No Members", "Please add members to this group before adding expenses.", "OK");
            return;
        }

        var description = await DisplayPromptAsync("Add Expense", "Enter expense description:");
        if (!string.IsNullOrWhiteSpace(description))
        {
            var amountText = await DisplayPromptAsync("Amount", "Enter expense amount (e.g., 25.50):");
            if (decimal.TryParse(amountText, out decimal amount))
            {
                // Pick payer
                string payer = await DisplayActionSheet("Who paid?", "Cancel", null, groupMembers.Select(m => m.Name).ToArray());
                if (payer == null || payer == "Cancel") return;
                var payerMember = groupMembers.First(m => m.Name == payer);

                // Open participant selection page with complete expense flow
                var parameters = new Dictionary<string, object>
                {
                    { "allMembers", groupMembers },
                    { "group", _group },
                    { "description", description },
                    { "amount", amount },
                    { "payerMember", payerMember }
                };
                await Shell.Current.GoToAsync("ParticipantSelectionPage", parameters);
                // Note: We'll need to handle the result differently since Shell navigation doesn't return values
                // For now, we'll add the expense directly in the ParticipantSelectionPage
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
                    AppData.RemoveExpense(expense);
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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh the UI to show updated data
        OnPropertyChanged(nameof(Expenses));
        UpdateAddButtonState();
    }
} 