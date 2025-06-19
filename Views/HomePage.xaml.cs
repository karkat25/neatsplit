using NeatSplit.ViewModels;
using NeatSplit.Services;

namespace NeatSplit.Views;

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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadGroups();
    }

    private async void OnAddGroupClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("AddGroupPage");
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
                    _viewModel.LoadGroups();
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
                    _viewModel.LoadGroups();
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to edit group: {ex.Message}", "OK");
        }
        finally { _isBusy = false; }
    }

    private async void OnGroupTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is Group group)
        {
            var parameters = new Dictionary<string, object>
            {
                { "groupId", group.Id }
            };
            await Shell.Current.GoToAsync("GroupDetailPage", parameters);
        }
    }

    private async void OnAddMemberClicked(object sender, EventArgs e)
    {
        var memberName = await DisplayPromptAsync("Add Member", "Enter member name:");
        if (!string.IsNullOrWhiteSpace(memberName))
        {
            var db = App.Current.Services.GetService<NeatSplitDatabase>();
            var member = new Member { Name = memberName.Trim() };
            await db.AddMemberAsync(member);
            _viewModel.LoadGroups();
        }
    }
} 