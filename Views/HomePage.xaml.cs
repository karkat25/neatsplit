namespace NeatSplit.Views;

using NeatSplit.Models;

public partial class HomePage : ContentPage
{
    private int _nextId = 1;
    public HomePage()
    {
        InitializeComponent();
        if (AppData.Groups.Count > 0)
            _nextId = AppData.Groups.Max(g => g.Id) + 1;
    }

    private async void OnCreateGroupClicked(object sender, EventArgs e)
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
            AppData.Groups.Add(newGroup);
            await DisplayAlert("Success", $"Group '{groupName}' created!", "OK");
        }
    }

    private async void OnViewGroupsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GroupsPage());
    }

    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Add Expense", "This will add an expense", "OK");
    }

    private async void OnViewBalancesClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Balances", "This will show balances", "OK");
    }
} 