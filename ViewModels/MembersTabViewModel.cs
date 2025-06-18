using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NeatSplit.Models;
using NeatSplit.Services;

namespace neatsplit.ViewModels
{
    public class MembersTabViewModel : BaseViewModel
    {
        private readonly NeatSplitDatabase _database;
        private int _groupId;
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
            foreach (var m in members)
                Members.Add(m);
        }

        public async Task<bool> AddMemberAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            var member = new Member { GroupId = _groupId, Name = name };
            await _database.SaveMemberAsync(member);
            Members.Add(member);
            return true;
        }
    }
} 