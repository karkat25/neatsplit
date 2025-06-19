using NeatSplit.ViewModels;
using NeatSplit.Services;
using Microsoft.Maui.Controls;

namespace NeatSplit.Views
{
    [QueryProperty(nameof(GroupId), "GroupId")]
    public partial class AddExpensePage : ContentPage
    {
        private readonly AddExpensePageViewModel _viewModel;
        private readonly int _groupId;

        public int GroupId
        {
            get => _groupId;
            set
            {
                _groupId = value;
                _viewModel = new AddExpensePageViewModel(App.Current.Services.GetService<NeatSplitDatabase>(), _groupId);
                BindingContext = _viewModel;
            }
        }

        public AddExpensePage(int groupId)
        {
            InitializeComponent();
            _groupId = groupId;
            _viewModel = new AddExpensePageViewModel(App.Current.Services.GetService<NeatSplitDatabase>(), _groupId);
            BindingContext = _viewModel;
            LoadMembers();
        }

        private async void LoadMembers()
        {
            var db = App.Current.Services.GetService<NeatSplitDatabase>();
            var members = await db.GetMembersAsync();
            PayerPicker.ItemsSource = members;
        }

        private async void OnAddItemClicked(object sender, EventArgs e)
        {
            var description = await DisplayPromptAsync("Add Item", "Enter item description:");
            if (!string.IsNullOrWhiteSpace(description))
            {
                var amountStr = await DisplayPromptAsync("Add Item", "Enter item amount:");
                if (double.TryParse(amountStr, out double amount))
                {
                    var item = new ExpenseItem
                    {
                        Description = description.Trim(),
                        Amount = amount
                    };
                    _viewModel.AddItem(item);
                }
            }
        }

        private async void OnEditItemClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ExpenseItem item)
            {
                var description = await DisplayPromptAsync("Edit Item", "Enter new description:", initialValue: item.Description);
                if (!string.IsNullOrWhiteSpace(description))
                {
                    var amountStr = await DisplayPromptAsync("Edit Item", "Enter new amount:", initialValue: item.Amount.ToString());
                    if (double.TryParse(amountStr, out double amount))
                    {
                        item.Description = description.Trim();
                        item.Amount = amount;
                        _viewModel.UpdateItem(item);
                    }
                }
            }
        }

        private void OnDeleteItemClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ExpenseItem item)
            {
                _viewModel.RemoveItem(item);
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a description", "OK");
                return;
            }

            if (!double.TryParse(AmountEntry.Text, out double totalAmount) || totalAmount <= 0)
            {
                await DisplayAlert("Error", "Please enter a valid amount", "OK");
                return;
            }

            if (PayerPicker.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please select a payer", "OK");
                return;
            }

            _viewModel.SelectedPayer = PayerPicker.SelectedItem as NeatSplit.Models.Member;
            _viewModel.Description = DescriptionEntry.Text.Trim();
            _viewModel.TotalAmount = totalAmount;
            _viewModel.Date = DatePicker.Date;

            await _viewModel.SaveExpenseAsync();
            await Shell.Current.GoToAsync("..");
        }
    }
} 