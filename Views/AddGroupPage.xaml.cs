using NeatSplit.Models;
using NeatSplit.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace neatsplit.Views
{
    public partial class AddGroupPage : ContentPage
    {
        private readonly NeatSplitDatabase _database;

        public AddGroupPage()
        {
            InitializeComponent();
            _database = App.Current.Services.GetService<NeatSplitDatabase>();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var groupName = GroupNameEntry.Text?.Trim();
            if (string.IsNullOrEmpty(groupName))
            {
                await Snackbar.Make("Please enter a group name.", null, "OK", TimeSpan.FromSeconds(3)).Show();
                return;
            }

            try
            {
                var group = new Group { Name = groupName };
                await _database.SaveGroupAsync(group);
                await Toast.Make($"Group '{groupName}' created!", ToastDuration.Short).Show();
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Snackbar.Make($"Error: {ex.Message}", null, "OK", TimeSpan.FromSeconds(3)).Show();
            }
        }
    }
} 