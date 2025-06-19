using NeatSplit.ViewModels;
using NeatSplit.Services;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace NeatSplit.Views
{
    public partial class ExpensesTab : ContentPage
    {
        private readonly ExpensesTabViewModel _viewModel;
        private readonly int _groupId;
        private bool _isBusy = false;

        public ExpensesTab(int groupId)
        {
            InitializeComponent();
            _groupId = groupId;
            _viewModel = new ExpensesTabViewModel(App.Current.Services.GetService<NeatSplitDatabase>(), _groupId);
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadExpenses();
        }

        private async void OnAddExpenseClicked(object sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "groupId", _groupId }
                };
                await Shell.Current.GoToAsync("AddExpensePage", parameters);
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
                if (sender is Button button && button.CommandParameter is int expenseId)
                {
                    var result = await DisplayAlert("Delete Expense", "Are you sure you want to delete this expense?", "Yes", "No");
                    if (result)
                    {
                        var db = App.Current.Services.GetService<NeatSplitDatabase>();
                        var expense = await db.GetExpenseAsync(expenseId);
                        if (expense != null)
                        {
                            await db.DeleteExpenseAsync(expense);
                            _viewModel.LoadExpenses();
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
                if (sender is Button button && button.CommandParameter is int expenseId)
                {
                    var db = App.Current.Services.GetService<NeatSplitDatabase>();
                    var expense = await db.GetExpenseAsync(expenseId);
                    if (expense != null)
                    {
                        var newDescription = await DisplayPromptAsync("Edit Expense", "Enter new description:", initialValue: expense.Description);
                        if (!string.IsNullOrWhiteSpace(newDescription))
                        {
                            expense.Description = newDescription.Trim();
                            await db.SaveExpenseAsync(expense);
                            _viewModel.LoadExpenses();
                            await Toast.Make("Expense updated.", ToastDuration.Short).Show();
                        }
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