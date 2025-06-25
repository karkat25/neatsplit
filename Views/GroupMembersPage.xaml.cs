using NeatSplit.Models;
using System.Collections.ObjectModel;

namespace NeatSplit.Views;

public partial class GroupMembersPage : ContentPage
{
    public ObservableCollection<Member> Members { get; set; }
    private int _nextId = 1;
    private readonly Group _group;

    public GroupMembersPage(Group group)
    {
        InitializeComponent();
        _group = group;
        Title = $"Members of {group.Name}";
        Members = new ObservableCollection<Member>(AppData.Members.Where(m => m.GroupId == group.Id));
        BindingContext = this;
        if (AppData.Members.Any())
            _nextId = AppData.Members.Max(m => m.Id) + 1;
    }

    private async void OnAddMemberClicked(object sender, EventArgs e)
    {
        var memberName = await DisplayPromptAsync("Add Member", "Enter member name:");
        if (!string.IsNullOrWhiteSpace(memberName))
        {
            var newMember = new Member
            {
                Id = _nextId++,
                Name = memberName.Trim(),
                GroupId = _group.Id
            };
            AppData.Members.Add(newMember);
            Members.Add(newMember);
            await DisplayAlert("Success", $"Member '{memberName}' added!", "OK");
        }
    }
} 