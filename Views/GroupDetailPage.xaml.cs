using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using NeatSplit.Services;

namespace NeatSplit.Views
{
    [QueryProperty(nameof(GroupId), "GroupId")]
    public partial class GroupDetailPage : TabbedPage
    {
        private readonly NeatSplitDatabase _database;
        private int _groupId;

        public int GroupId
        {
            get => _groupId;
            set
            {
                _groupId = value;
                // TODO: Load group details as needed
            }
        }

        public GroupDetailPage()
        {
            InitializeComponent();
            _database = App.Current.Services.GetService<NeatSplitDatabase>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Get group ID from query parameters
            if (Shell.Current.CurrentState.Location.ToString().Contains("GroupId="))
            {
                var queryString = Shell.Current.CurrentState.Location.ToString().Split('?')[1];
                var parameters = queryString.Split('&')
                    .Select(p => p.Split('='))
                    .ToDictionary(p => p[0], p => p[1]);
                
                if (parameters.ContainsKey("GroupId") && int.TryParse(parameters["GroupId"], out int groupId))
                {
                    _groupId = groupId;
                    await LoadGroupDetails();
                }
            }
        }

        private async Task LoadGroupDetails()
        {
            var group = await _database.GetGroupAsync(_groupId);
            if (group != null)
            {
                GroupNameLabel.Text = group.Name;
                GroupDescriptionLabel.Text = group.Description;
                
                var members = await _database.GetMembersAsync();
                MemberCountLabel.Text = $"Members: {members.Count}";
            }
        }
    }
} 