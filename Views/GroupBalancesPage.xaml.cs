using NeatSplit.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NeatSplit.Views;

public class PaymentInstruction : INotifyPropertyChanged
{
    private bool _isPaid;
    private ObservableCollection<PaymentInstruction>? _parentCollection;
    
    public string FromPerson { get; set; } = string.Empty;
    public string ToPerson { get; set; } = string.Empty;
    public int FromMemberId { get; set; }
    public int ToMemberId { get; set; }
    public int GroupId { get; set; }
    public decimal Amount { get; set; }
    
    public void SetParentCollection(ObservableCollection<PaymentInstruction> collection)
    {
        _parentCollection = collection;
    }
    
    public bool IsPaid 
    { 
        get => _isPaid;
        set
        {
            if (_isPaid != value)
            {
                _isPaid = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                
                // Update the paid payments collection
                if (value)
                {
                    // Mark as paid
                    var existingPayment = AppData.PaidPayments.FirstOrDefault(p => 
                        p.FromMemberId == FromMemberId && 
                        p.ToMemberId == ToMemberId && 
                        p.GroupId == GroupId && 
                        Math.Abs(p.Amount - Amount) < 0.01m);
                    
                    if (existingPayment == null)
                    {
                        AppData.PaidPayments.Add(new PaidPayment
                        {
                            FromMemberId = FromMemberId,
                            ToMemberId = ToMemberId,
                            GroupId = GroupId,
                            Amount = Amount,
                            PaidDate = DateTime.Now
                        });
                    }
                    
                    // Remove from collection immediately
                    if (_parentCollection != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            _parentCollection.Remove(this);
                        });
                    }
                }
                else
                {
                    // Mark as unpaid
                    var paymentToRemove = AppData.PaidPayments.FirstOrDefault(p => 
                        p.FromMemberId == FromMemberId && 
                        p.ToMemberId == ToMemberId && 
                        p.GroupId == GroupId && 
                        Math.Abs(p.Amount - Amount) < 0.01m);
                    
                    if (paymentToRemove != null)
                    {
                        AppData.PaidPayments.Remove(paymentToRemove);
                    }
                }
            }
        }
    }
    
    public string DisplayText => $"{FromPerson} needs to pay ${Amount:F2} to {ToPerson}";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class MemberBalanceInfo
{
    public Member Member { get; set; } = null!;
    public decimal Balance { get; set; }
}

[QueryProperty(nameof(Group), "group")]
public partial class GroupBalancesPage : ContentPage
{
    public ObservableCollection<PaymentInstruction> Balances { get; set; }
    public string EmptyMessage { get; set; } = string.Empty;
    private Group _group = null!;

    public Group Group
    {
        get => _group;
        set
        {
            _group = value;
            Title = $"Balances - {_group.Name}";
            
            var members = AppData.Members.Where(m => m.GroupId == _group.Id).ToList();
            var expenses = AppData.Expenses.Where(e => e.GroupId == _group.Id).ToList();
            
            if (members.Count == 0)
            {
                EmptyMessage = "Add members to this group first!";
            }
            else if (expenses.Count == 0)
            {
                EmptyMessage = "Add some expenses to see payment balances!";
            }
            else
            {
                EmptyMessage = "All expenses are evenly split!";
            }
            
            var paymentInstructions = CalculatePaymentInstructions(_group);
            // Sort: unpaid first, then paid
            var sortedInstructions = paymentInstructions
                .OrderBy(p => p.IsPaid)
                .ThenBy(p => p.Amount)
                .ToList();
                
            Balances = new ObservableCollection<PaymentInstruction>(sortedInstructions);
            
            // Set parent collection reference for each instruction
            foreach (var instruction in Balances)
            {
                instruction.SetParentCollection(Balances);
            }
            
            BindingContext = this;
        }
    }

    public GroupBalancesPage()
    {
        InitializeComponent();
        Balances = new ObservableCollection<PaymentInstruction>();
    }

    private List<PaymentInstruction> CalculatePaymentInstructions(Group group)
    {
        var members = AppData.Members.Where(m => m.GroupId == group.Id).ToList();
        var expenses = AppData.Expenses.Where(e => e.GroupId == group.Id).ToList();
        
        if (members.Count == 0 || expenses.Count == 0)
            return new List<PaymentInstruction>();

        // Calculate what each person paid
        var paidBy = members.ToDictionary(m => m.Id, m => expenses.Where(e => e.PaidByMemberId == m.Id).Sum(e => e.Amount));
        
        // Calculate what each person owes based on their participation in each expense
        var owedBy = new Dictionary<int, decimal>();
        foreach (var member in members)
        {
            var totalOwed = 0m;
            foreach (var expense in expenses)
            {
                if (expense.Participants.Contains(member.Id))
                {
                    var participantCount = expense.Participants.Count;
                    if (participantCount > 0)
                    {
                        totalOwed += expense.Amount / participantCount;
                    }
                }
            }
            owedBy[member.Id] = totalOwed;
        }
        
        // Calculate initial balances (positive = they paid more, negative = they owe money)
        var balances = members.ToDictionary(m => m.Id, m => paidBy[m.Id] - owedBy[m.Id]);
        
        // Subtract paid payments from the balances
        var paidPayments = AppData.PaidPayments.Where(p => p.GroupId == group.Id).ToList();
        foreach (var payment in paidPayments)
        {
            if (balances.ContainsKey(payment.FromMemberId) && balances.ContainsKey(payment.ToMemberId))
            {
                balances[payment.FromMemberId] += payment.Amount; // Person who paid gets credited
                balances[payment.ToMemberId] -= payment.Amount;   // Person who received gets debited
            }
        }
        
        // Convert to list for processing
        var balanceList = balances.Select(kvp => new MemberBalanceInfo 
        { 
            Member = members.First(m => m.Id == kvp.Key), 
            Balance = kvp.Value 
        }).ToList();
        
        var instructions = new List<PaymentInstruction>();
        var debtors = balanceList.Where(b => b.Balance < -0.01m).OrderBy(b => b.Balance).ToList(); // People who owe money
        var creditors = balanceList.Where(b => b.Balance > 0.01m).OrderByDescending(b => b.Balance).ToList(); // People who are owed money
        
        int debtorIndex = 0;
        int creditorIndex = 0;
        
        while (debtorIndex < debtors.Count && creditorIndex < creditors.Count)
        {
            var debtor = debtors[debtorIndex];
            var creditor = creditors[creditorIndex];
            
            decimal paymentAmount = Math.Min(Math.Abs(debtor.Balance), creditor.Balance);
            
            if (paymentAmount > 0.01m) // Only show payments over 1 cent
            {
                // Check if this payment is already marked as paid
                var isAlreadyPaid = AppData.PaidPayments.Any(p => 
                    p.FromMemberId == debtor.Member.Id && 
                    p.ToMemberId == creditor.Member.Id && 
                    p.GroupId == group.Id && 
                    Math.Abs(p.Amount - paymentAmount) < 0.01m);
                
                instructions.Add(new PaymentInstruction
                {
                    FromPerson = debtor.Member.Name,
                    ToPerson = creditor.Member.Name,
                    FromMemberId = debtor.Member.Id,
                    ToMemberId = creditor.Member.Id,
                    GroupId = group.Id,
                    Amount = paymentAmount,
                    IsPaid = isAlreadyPaid
                });
            }
            
            debtor.Balance += paymentAmount;
            creditor.Balance -= paymentAmount;
            
            if (Math.Abs(debtor.Balance) < 0.01m) debtorIndex++;
            if (creditor.Balance < 0.01m) creditorIndex++;
        }
        
        return instructions;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh the UI to show updated data
        OnPropertyChanged(nameof(Balances));
    }
} 