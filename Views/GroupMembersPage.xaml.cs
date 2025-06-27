using NeatSplit.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace NeatSplit.Views;

[QueryProperty(nameof(Group), "group")]
public partial class GroupMembersPage : ContentPage
{
    public ObservableCollection<Member> Members { get; set; }
    private int _nextId = 1;
    private Group _group = null!;

    public Group Group
    {
        get => _group;
        set
        {
            _group = value;
            Title = $"Members - {_group.Name}";
            Members = new ObservableCollection<Member>(AppData.Members.Where(m => m.GroupId == _group.Id));
            if (AppData.Members.Any())
                _nextId = AppData.Members.Max(m => m.Id) + 1;
            BindingContext = this;
        }
    }

    public GroupMembersPage()
    {
        InitializeComponent();
        Members = new ObservableCollection<Member>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh the UI to show updated data
        OnPropertyChanged(nameof(Members));
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
            AppData.AddMember(newMember);
            Members.Add(newMember);
            await DisplayAlert("Success", $"Member '{memberName}' added!", "OK");
        }
    }

    private async void OnEditMemberClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int memberId)
        {
            var member = Members.FirstOrDefault(m => m.Id == memberId);
            if (member != null)
            {
                string newName = await DisplayPromptAsync("Edit Member", "Edit member name:", initialValue: member.Name);
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    member.Name = newName.Trim();
                    // Refresh UI
                    var idx = Members.IndexOf(member);
                    if (idx >= 0)
                    {
                        Members.RemoveAt(idx);
                        Members.Insert(idx, member);
                    }
                    await DisplayAlert("Success", "Member updated!", "OK");
                }
            }
        }
    }

    private async void OnDeleteMemberClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int memberId)
        {
            var member = Members.FirstOrDefault(m => m.Id == memberId);
            if (member != null)
            {
                bool confirm = await DisplayAlert("Delete Member", $"Are you sure you want to delete '{member.Name}'?", "Yes", "No");
                if (confirm)
                {
                    Members.Remove(member);
                    AppData.RemoveMember(member);
                    await DisplayAlert("Deleted", $"Member '{member.Name}' has been deleted.", "OK");
                }
            }
        }
    }
} 