using NeatSplit.Models;
using System.Collections.ObjectModel;

namespace NeatSplit.Views;

public partial class GroupsPage : ContentPage
{
    public ObservableCollection<Group> Groups => AppData.Groups;
    private int _nextId = 1;

    public GroupsPage()
    {
        InitializeComponent();
        BindingContext = this;
        if (Groups.Count > 0)
            _nextId = Groups.Max(g => g.Id) + 1;
    }

    private async void OnAddGroupClicked(object sender, EventArgs e)
    {
        var groupName = await DisplayPromptAsync("Create Group", "Enter group name:");
        if (!string.IsNullOrWhiteSpace(groupName))
        {
            var description = await DisplayPromptAsync("Description", "Enter group description (optional):");
            var newGroup = new Group
            {
                Id = _nextId++,
                Name = groupName.Trim(),
                Description = description?.Trim() ?? "",
                CreatedDate = DateTime.Now
            };
            Groups.Add(newGroup);
            await DisplayAlert("Success", $"Group '{groupName}' created!", "OK");
        }
    }

    private async void OnDeleteGroupClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int groupId)
        {
            var group = Groups.FirstOrDefault(g => g.Id == groupId);
            if (group != null)
            {
                bool confirm = await DisplayAlert("Delete Group", $"Are you sure you want to delete '{group.Name}'?", "Yes", "No");
                if (confirm)
                {
                    Groups.Remove(group);
                    await DisplayAlert("Deleted", $"Group '{group.Name}' has been deleted.", "OK");
                }
            }
        }
    }

    private async void OnGroupSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Group selectedGroup)
        {
            await Navigation.PushAsync(new GroupMembersPage(selectedGroup));
            GroupsCollectionView.SelectedItem = null;
        }
    }

    private async void OnExpensesClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int groupId)
        {
            var group = Groups.FirstOrDefault(g => g.Id == groupId);
            if (group != null)
            {
                await Navigation.PushAsync(new GroupExpensesPage(group));
            }
        }
    }

    private async void OnBalancesClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int groupId)
        {
            var group = Groups.FirstOrDefault(g => g.Id == groupId);
            if (group != null)
            {
                await Navigation.PushAsync(new GroupBalancesPage(group));
            }
        }
    }

    private async void OnMembersClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int groupId)
        {
            var group = Groups.FirstOrDefault(g => g.Id == groupId);
            if (group != null)
            {
                await Navigation.PushAsync(new GroupMembersPage(group));
            }
        }
    }
} 