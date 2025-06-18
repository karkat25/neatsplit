using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using NeatSplit.Models;
using NeatSplit.Services;

namespace neatsplit.ViewModels
{
    public class GroupDisplay
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MemberCount { get; set; }
        public string UserBalance { get; set; } = "$0.00"; // Placeholder
    }

    public class HomePageViewModel : BaseViewModel
    {
        private readonly NeatSplitDatabase _database;
        public ObservableCollection<GroupDisplay> Groups { get; set; } = new();

        public HomePageViewModel(NeatSplitDatabase database)
        {
            _database = database;
            LoadGroupsCommand = new Command(async () => await LoadGroupsAsync());
        }

        public Command LoadGroupsCommand { get; }

        public async Task LoadGroupsAsync()
        {
            Groups.Clear();
            var groups = await _database.GetGroupsAsync();
            foreach (var group in groups)
            {
                var members = await _database.GetMembersForGroupAsync(group.Id);
                Groups.Add(new GroupDisplay
                {
                    Id = group.Id,
                    Name = group.Name,
                    MemberCount = members.Count,
                    UserBalance = "$0.00" // Placeholder
                });
            }
        }
    }
} 