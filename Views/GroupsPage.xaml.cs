using NeatSplit.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace NeatSplit.Views;

public partial class GroupsPage : ContentPage
{
    public ObservableCollection<Group> Groups => AppData.Groups;
    private int _nextId = 1;

    public GroupsPage()
    {
        InitializeComponent();
        if (Groups.Count > 0)
            _nextId = Groups.Max(g => g.Id) + 1;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh the UI to show updated data
        OnPropertyChanged(nameof(Groups));
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
            var parameters = new Dictionary<string, object>
            {
                { "group", selectedGroup }
            };
            await Shell.Current.GoToAsync("GroupMembersPage", parameters);
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
                var parameters = new Dictionary<string, object>
                {
                    { "group", group }
                };
                await Shell.Current.GoToAsync("GroupExpensesPage", parameters);
            }
        }
    }

    private async void OnBalancesClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int groupId)
        {
            var group = Groups.FirstOrDefault(g => g.Id == groupId);
            if (group != null)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "group", group }
                };
                await Shell.Current.GoToAsync("GroupBalancesPage", parameters);
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
                var parameters = new Dictionary<string, object>
                {
                    { "group", group }
                };
                await Shell.Current.GoToAsync("GroupMembersPage", parameters);
            }
        }
    }

    private async void OnGroupTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is Group group)
        {
            string action = await DisplayActionSheet($"Options for '{group.Name}'", "Cancel", null, "Edit", "Delete");
            if (action == "Edit")
            {
                string newName = await DisplayPromptAsync("Edit Group", "Edit group name:", initialValue: group.Name);
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    string newDesc = await DisplayPromptAsync("Edit Description", "Edit group description (optional):", initialValue: group.Description);
                    group.Name = newName.Trim();
                    group.Description = newDesc?.Trim() ?? "";
                    // Refresh UI
                    var idx = Groups.IndexOf(group);
                    if (idx >= 0)
                    {
                        Groups.RemoveAt(idx);
                        Groups.Insert(idx, group);
                    }
                    await DisplayAlert("Success", "Group updated!", "OK");
                }
            }
            else if (action == "Delete")
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
} 