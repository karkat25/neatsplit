using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NeatSplit.Models;

public class Group : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    
    public int Id { get; set; }
    
    public string Name 
    { 
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string Description 
    { 
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
            }
        }
    }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public decimal TotalExpenses =>
        NeatSplit.AppData.Expenses.Where(e => e.GroupId == Id).Sum(e => e.Amount);
        
    public int MemberCount =>
        NeatSplit.AppData.Members.Count(m => m.GroupId == Id);

    public void NotifyCalculatedPropertiesChanged()
    {
        OnPropertyChanged(nameof(TotalExpenses));
        OnPropertyChanged(nameof(MemberCount));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 