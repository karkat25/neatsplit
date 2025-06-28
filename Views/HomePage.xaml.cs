using NeatSplit.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace NeatSplit.Views;

public partial class HomePage : ContentPage
{
    public ObservableCollection<Group> Groups => AppData.Groups;

    public HomePage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh the UI to show updated stats
        OnPropertyChanged(nameof(Groups));
    }

    private async void OnCreateGroupClicked(object sender, EventArgs e)
    {
        try
        {
            var groupName = await DisplayPromptAsync("Create Group", "Enter group name:");
            if (!string.IsNullOrWhiteSpace(groupName))
            {
                var description = await DisplayPromptAsync("Description", "Enter group description (optional):");
                var newGroup = new Group
                {
                    Name = groupName.Trim(),
                    Description = description?.Trim() ?? "",
                    CreatedDate = DateTime.Now
                };
                
                System.Diagnostics.Debug.WriteLine($"Creating group: {newGroup.Name}");
                await AppData.AddGroupAsync(newGroup);
                await DisplayAlert("Success", $"Group '{groupName}' created!", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating group: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", $"Failed to create group: {ex.Message}", "OK");
        }
    }

    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        if (Groups.Count == 0)
        {
            await DisplayAlert("No Groups", "Please create a group first.", "OK");
            return;
        }

        // Select group
        var groupNames = Groups.Select(g => g.Name).ToArray();
        var selectedGroupName = await DisplayActionSheet("Select Group", "Cancel", null, groupNames);
        if (selectedGroupName == null || selectedGroupName == "Cancel") return;

        var selectedGroup = Groups.First(g => g.Name == selectedGroupName);
        var groupMembers = AppData.Members.Where(m => m.GroupId == selectedGroup.Id).ToList();

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
                    { "group", selectedGroup },
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
        if (sender is Button btn && btn.CommandParameter is int groupId)
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
                    await DisplayAlert("Success", "Group updated!", "OK");
                }
            }
            else if (action == "Delete")
            {
                bool confirm = await DisplayAlert("Delete Group", $"Are you sure you want to delete '{group.Name}'?", "Yes", "No");
                if (confirm)
                {
                    await AppData.DeleteGroupAsync(group);
                    await DisplayAlert("Deleted", $"Group '{group.Name}' has been deleted.", "OK");
                }
            }
        }
    }
} 