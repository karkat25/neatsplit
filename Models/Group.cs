using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace NeatSplit.Models;

[Table("Groups")]
public class Group : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [MaxLength(100)]
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
    
    [MaxLength(500)]
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

    [Ignore]
    public decimal TotalExpenses
    {
        get
        {
            var total = NeatSplit.AppData.Expenses.Where(e => e.GroupId == Id).Sum(e => e.Amount);
            return total;
        }
    }
        
    [Ignore]
    public int MemberCount
    {
        get
        {
            var count = NeatSplit.AppData.Members.Count(m => m.GroupId == Id);
            return count;
        }
    }

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