using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NeatSplit.Models;
using NeatSplit.Services;

namespace NeatSplit.ViewModels
{
    public class MembersTabViewModel : BaseViewModel
    {
        private readonly NeatSplitDatabase _database;
        private readonly int _groupId;
        public ObservableCollection<Member> Members { get; set; } = new();

        public MembersTabViewModel(NeatSplitDatabase database, int groupId)
        {
            _database = database;
            _groupId = groupId;
        }

        public async Task LoadMembersAsync()
        {
            Members.Clear();
            var members = await _database.GetMembersForGroupAsync(_groupId);
            foreach (var member in members)
            {
                Members.Add(member);
            }
        }

        public void LoadMembers()
        {
            _ = LoadMembersAsync();
        }

        public async Task<(bool success, string errorMessage)> AddMemberAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Member name cannot be empty.");

            try
            {
                var member = new Member { Id = 0, GroupId = _groupId, Name = name, CreatedDate = DateTime.Now };
                await _database.SaveMemberAsync(member);
                Members.Add(member);
                return (true, string.Empty);
            }
            catch (InvalidOperationException ex)
            {
                // Handle duplicate member name error
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                // Handle other database errors
                return (false, $"Failed to add member: {ex.Message}");
            }
        }
    }
} 