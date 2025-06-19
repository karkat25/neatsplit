using NeatSplit.Models;
using NeatSplit.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace NeatSplit.Views;

public partial class AddGroupPage : ContentPage
{
    private readonly NeatSplitDatabase _database;

    public AddGroupPage()
    {
        InitializeComponent();
        _database = App.Current.Services.GetService<NeatSplitDatabase>();
    }

    private async void OnCreateGroupClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GroupNameEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a group name", "OK");
            return;
        }

        var group = new Group
        {
            Id = 0,
            Name = GroupNameEntry.Text.Trim(),
            Description = DescriptionEntry.Text?.Trim() ?? "",
            CreatedDate = DateTime.Now
        };
        try
        {
            await _database.AddGroupAsync(group);
            await Shell.Current.GoToAsync("..");
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlert("Duplicate Group", ex.Message, "OK");
        }
    }
} 