using NeatSplit.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NeatSplit.Views;

public class ParticipantItem : INotifyPropertyChanged
{
    private bool _isSelected;
    
    public Member Member { get; set; } = null!;
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

[QueryProperty(nameof(AllMembers), "allMembers")]
[QueryProperty(nameof(Group), "group")]
[QueryProperty(nameof(Description), "description")]
[QueryProperty(nameof(Amount), "amount")]
[QueryProperty(nameof(PayerMember), "payerMember")]
public partial class ParticipantSelectionPage : ContentPage
{
    public ObservableCollection<ParticipantItem> Participants { get; set; } = new();
    private TaskCompletionSource<Expense?> _tcs;
    private bool _resultSet = false;
    private Group _group = null!;
    private string _description = string.Empty;
    private decimal _amount;
    private Member _payerMember = null!;
    private List<Member> _allMembers = new();

    public List<Member> AllMembers
    {
        get => _allMembers;
        set
        {
            _allMembers = value;
            // Initialize participants with all members selected by default
            Participants.Clear();
            foreach (var member in _allMembers)
            {
                Participants.Add(new ParticipantItem { Member = member, IsSelected = true });
            }
            ParticipantsListView.ItemsSource = Participants;
        }
    }

    public Group Group
    {
        get => _group;
        set => _group = value;
    }

    public string Description
    {
        get => _description;
        set => _description = value;
    }

    public decimal Amount
    {
        get => _amount;
        set => _amount = value;
    }

    public Member PayerMember
    {
        get => _payerMember;
        set => _payerMember = value;
    }

    public ParticipantSelectionPage()
    {
        InitializeComponent();
        BindingContext = this;
        _tcs = new TaskCompletionSource<Expense?>();
    }

    public Task<Expense?> GetExpenseAsync()
    {
        return _tcs.Task;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (!_resultSet)
        {
            _tcs.TrySetResult(null);
            _resultSet = true;
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        if (!_resultSet)
        {
            _tcs.SetResult(null);
            _resultSet = true;
        }
        await Navigation.PopAsync();
    }

    private async void OnDoneClicked(object sender, EventArgs e)
    {
        var selectedMembers = Participants.Where(p => p.IsSelected).Select(p => p.Member).ToList();
        
        if (selectedMembers.Count == 0)
        {
            await DisplayAlert("No Participants", "Please select at least one participant.", "OK");
            return;
        }

        // Create the expense
        var expense = new Expense
        {
            Id = GetNextExpenseId(),
            GroupId = _group.Id,
            Description = _description.Trim(),
            Amount = _amount,
            Date = DateTime.Now,
            PaidByMemberId = _payerMember.Id,
            Participants = selectedMembers.Select(p => p.Id).ToList(),
            SplitType = SplitType.Equal // Default to equal split
        };

        // Ask if they want to change the split type (defaults to Equal Split)
        string splitType = await DisplayActionSheet("Split Type", "Equal Split (Default)", "Cancel", "Custom Split", "Percentage Split");
        
        if (splitType == "Cancel") return;

        // Handle different split types
        if (splitType == "Custom Split")
        {
            expense.SplitType = SplitType.Custom;
            if (!await SetupCustomSplit(expense, selectedMembers, _amount))
                return;
        }
        else if (splitType == "Percentage Split")
        {
            expense.SplitType = SplitType.Percentage;
            if (!await SetupPercentageSplit(expense, selectedMembers))
                return;
        }
        // If splitType is null or "Equal Split (Default)", it stays as Equal Split

        // Add the expense directly
        AppData.AddExpense(expense);
        await DisplayAlert("Success", $"Expense '{_description}' added!", "OK");
        
        // Navigate back
        await Shell.Current.GoToAsync("..");
    }

    private int GetNextExpenseId()
    {
        if (AppData.Expenses.Count > 0)
            return AppData.Expenses.Max(e => e.Id) + 1;
        return 1;
    }

    private async Task<bool> SetupCustomSplit(Expense expense, List<Member> members, decimal totalAmount)
    {
        var customAmounts = new Dictionary<int, decimal>();
        decimal enteredTotal = 0;

        foreach (var member in members)
        {
            var amountText = await DisplayPromptAsync($"Amount for {member.Name}", 
                $"Enter amount for {member.Name} (Total: ${totalAmount:F2}, Remaining: ${(totalAmount - enteredTotal):F2}):", 
                initialValue: (totalAmount / members.Count).ToString("F2"));
            
            if (string.IsNullOrWhiteSpace(amountText) || !decimal.TryParse(amountText, out decimal amount))
            {
                await DisplayAlert("Error", "Please enter a valid amount.", "OK");
                return false;
            }

            customAmounts[member.Id] = amount;
            enteredTotal += amount;
        }

        if (Math.Abs(enteredTotal - totalAmount) > 0.01m)
        {
            await DisplayAlert("Error", $"Total entered (${enteredTotal:F2}) doesn't match expense amount (${totalAmount:F2}).", "OK");
            return false;
        }

        expense.CustomSplitAmounts = customAmounts;
        return true;
    }

    private async Task<bool> SetupPercentageSplit(Expense expense, List<Member> members)
    {
        var percentageAmounts = new Dictionary<int, decimal>();
        decimal enteredTotal = 0;

        foreach (var member in members)
        {
            var percentageText = await DisplayPromptAsync($"Percentage for {member.Name}", 
                $"Enter percentage for {member.Name} (Total: 100%, Remaining: {100 - enteredTotal:F1}%):", 
                initialValue: (100.0m / members.Count).ToString("F1"));
            
            if (string.IsNullOrWhiteSpace(percentageText) || !decimal.TryParse(percentageText, out decimal percentage))
            {
                await DisplayAlert("Error", "Please enter a valid percentage.", "OK");
                return false;
            }

            percentageAmounts[member.Id] = percentage;
            enteredTotal += percentage;
        }

        if (Math.Abs(enteredTotal - 100) > 0.1m)
        {
            await DisplayAlert("Error", $"Total percentage ({enteredTotal:F1}%) doesn't equal 100%.", "OK");
            return false;
        }

        expense.PercentageSplitAmounts = percentageAmounts;
        return true;
    }
} 