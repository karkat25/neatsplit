using neatsplit.ViewModels;
using NeatSplit.Services;

namespace neatsplit.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly HomePageViewModel _viewModel;
        private bool _isBusy = false;

        public HomePage()
        {
            InitializeComponent();
            _viewModel = new HomePageViewModel(App.Current.Services.GetService<NeatSplitDatabase>());
            BindingContext = _viewModel;
            GroupsCollectionView.SelectionChanged += OnGroupSelected;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadGroupsAsync();
        }

        private async void OnAddGroupClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddGroupPage());
        }

        private async void OnGroupSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ViewModels.GroupDisplay selectedGroup)
            {
                await Shell.Current.GoToAsync($"GroupDetailPage?GroupId={selectedGroup.Id}");
                GroupsCollectionView.SelectedItem = null;
            }
        }

        private async void OnDeleteGroupClicked(object sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                if (sender is Button btn && btn.CommandParameter is int groupId)
                {
                    bool confirm = await DisplayAlert("Delete Group", "Are you sure you want to delete this group? This will remove all related data.", "Yes", "No");
                    if (confirm)
                    {
                        var db = App.Current.Services.GetService<NeatSplitDatabase>();
                        var dbGroup = await db.GetGroupAsync(groupId);
                        await db.DeleteGroupAsync(dbGroup);
                        await _viewModel.LoadGroupsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete group: {ex.Message}", "OK");
            }
            finally { _isBusy = false; }
        }

        private async void OnEditGroupClicked(object sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                if (sender is Button btn && btn.CommandParameter is int groupId)
                {
                    var db = App.Current.Services.GetService<NeatSplitDatabase>();
                    var dbGroup = await db.GetGroupAsync(groupId);
                    string newName = await DisplayPromptAsync("Edit Group", "Enter new group name:", initialValue: dbGroup.Name);
                    if (!string.IsNullOrWhiteSpace(newName) && newName != dbGroup.Name)
                    {
                        dbGroup.Name = newName;
                        await db.SaveGroupAsync(dbGroup);
                        await _viewModel.LoadGroupsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to edit group: {ex.Message}", "OK");
            }
            finally { _isBusy = false; }
        }
    }
} 