using neatsplit.ViewModels;
using NeatSplit.Services;
using Microsoft.Maui.Controls;

namespace neatsplit.Views
{
    public partial class MembersTab : ContentPage
    {
        private MembersTabViewModel _viewModel;
        private int _groupId;
        private bool _isBusy = false;

        public MembersTab()
        {
            InitializeComponent();
        }

        protected override async void OnParentSet()
        {
            base.OnParentSet();
            if (Parent is GroupDetailPage groupDetailPage)
            {
                _groupId = groupDetailPage.GroupId;
                _viewModel = new MembersTabViewModel(App.Current.Services.GetService<NeatSplitDatabase>(), _groupId);
                BindingContext = _viewModel;
                await _viewModel.LoadMembersAsync();
            }
        }

        private async void OnAddMemberClicked(object sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                string name = await DisplayPromptAsync("Add Member", "Enter member name:");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    var success = await _viewModel.AddMemberAsync(name);
                    if (!success)
                        await DisplayAlert("Error", "Could not add member.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to add member: {ex.Message}", "OK");
            }
            finally { _isBusy = false; }
        }

        private async void OnDeleteMemberClicked(object sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                if (sender is Button btn && btn.CommandParameter is int memberId)
                {
                    bool confirm = await DisplayAlert("Delete Member", "Are you sure you want to delete this member?", "Yes", "No");
                    if (confirm)
                    {
                        var member = _viewModel.Members.FirstOrDefault(x => x.Id == memberId);
                        if (member != null)
                        {
                            var db = App.Current.Services.GetService<NeatSplitDatabase>();
                            var dbMember = await db.GetMemberAsync(memberId);
                            await db.DeleteMemberAsync(dbMember);
                            await _viewModel.LoadMembersAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete member: {ex.Message}", "OK");
            }
            finally { _isBusy = false; }
        }

        private async void OnEditMemberClicked(object sender, EventArgs e)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                if (sender is Button btn && btn.CommandParameter is int memberId)
                {
                    var db = App.Current.Services.GetService<NeatSplitDatabase>();
                    var dbMember = await db.GetMemberAsync(memberId);
                    string newName = await DisplayPromptAsync("Edit Member", "Enter new member name:", initialValue: dbMember.Name);
                    if (!string.IsNullOrWhiteSpace(newName) && newName != dbMember.Name)
                    {
                        dbMember.Name = newName;
                        await db.SaveMemberAsync(dbMember);
                        await _viewModel.LoadMembersAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to edit member: {ex.Message}", "OK");
            }
            finally { _isBusy = false; }
        }
    }
} 