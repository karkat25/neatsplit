using neatsplit.ViewModels;
using NeatSplit.Services;
using Microsoft.Maui.Controls;

namespace neatsplit.Views
{
    [QueryProperty(nameof(GroupId), "GroupId")]
    public partial class AddExpensePage : ContentPage
    {
        private AddExpensePageViewModel _viewModel;
        private int _groupId;
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

        public AddExpensePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.LoadMembersAsync();
                PayerPicker.ItemsSource = _viewModel.Members;
                PayerPicker.SelectedItem = null;
                _viewModel.Items.Clear();
            }
        }

        private void OnAddItemClicked(object sender, EventArgs e)
        {
            _viewModel.AddItem();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            _viewModel.Description = DescriptionEntry.Text;
            _viewModel.Total = TotalEntry.Text;
            _viewModel.SelectedPayer = PayerPicker.SelectedItem as NeatSplit.Models.Member;
            var success = await _viewModel.SaveExpenseAsync();
            if (success)
            {
                await DisplayAlert("Success", "Expense added!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", "Please fill all fields and add at least one item.", "OK");
            }
        }
    }
} 