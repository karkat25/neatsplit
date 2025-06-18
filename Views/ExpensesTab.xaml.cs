using neatsplit.ViewModels;
using NeatSplit.Services;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace neatsplit.Views
{
    public partial class ExpensesTab : ContentPage
    {
        private ExpensesTabViewModel _viewModel;
        private int _groupId;
        private bool _isBusy = false;

        public ExpensesTab()
        {
            InitializeComponent();
        }

        protected override async void OnParentSet()
        {
            base.OnParentSet();
            if (Parent is GroupDetailPage groupDetailPage)
            {
                _groupId = groupDetailPage.GroupId;
                _viewModel = new ExpensesTabViewModel(App.Current.Services.GetService<NeatSplitDatabase>(), _groupId);
                BindingContext = _viewModel;
                await _viewModel.LoadExpensesAsync();
            }
        }

        private async void OnAddExpenseClicked(object sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                await Shell.Current.GoToAsync($"AddExpensePage?GroupId={_groupId}");
            }
            catch (Exception ex)
            {
                await Snackbar.Make($"Failed to navigate: {ex.Message}", null, "OK", TimeSpan.FromSeconds(3)).Show();
            }
            finally { _isBusy = false; }
        }

        private async void OnDeleteExpenseClicked(object sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                if (sender is Button btn && btn.CommandParameter is int expenseId)
                {
                    bool confirm = await DisplayAlert("Delete Expense", "Are you sure you want to delete this expense?", "Yes", "No");
                    if (confirm)
                    {
                        var expense = _viewModel.Expenses.FirstOrDefault(x => x.Id == expenseId);
                        if (expense != null)
                        {
                            var db = App.Current.Services.GetService<NeatSplitDatabase>();
                            var dbExpense = await db.GetExpenseAsync(expenseId);
                            await db.DeleteExpenseAsync(dbExpense);
                            await _viewModel.LoadExpensesAsync();
                            await Toast.Make("Expense deleted.", ToastDuration.Short).Show();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Snackbar.Make($"Failed to delete expense: {ex.Message}", null, "OK", TimeSpan.FromSeconds(3)).Show();
            }
            finally { _isBusy = false; }
        }

        private async void OnEditExpenseClicked(object sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                if (sender is Button btn && btn.CommandParameter is int expenseId)
                {
                    var db = App.Current.Services.GetService<NeatSplitDatabase>();
                    var dbExpense = await db.GetExpenseAsync(expenseId);
                    string newDescription = await DisplayPromptAsync("Edit Expense", "Enter new description:", initialValue: dbExpense.Description);
                    string newTotal = await DisplayPromptAsync("Edit Expense", "Enter new total amount:", initialValue: dbExpense.TotalAmount.ToString());
                    if (!string.IsNullOrWhiteSpace(newDescription) && double.TryParse(newTotal, out double total) && (newDescription != dbExpense.Description || total != dbExpense.TotalAmount))
                    {
                        dbExpense.Description = newDescription;
                        dbExpense.TotalAmount = total;
                        await db.SaveExpenseAsync(dbExpense);
                        await _viewModel.LoadExpensesAsync();
                        await Toast.Make("Expense updated.", ToastDuration.Short).Show();
                    }
                }
            }
            catch (Exception ex)
            {
                await Snackbar.Make($"Failed to edit expense: {ex.Message}", null, "OK", TimeSpan.FromSeconds(3)).Show();
            }
            finally { _isBusy = false; }
        }
    }
} 