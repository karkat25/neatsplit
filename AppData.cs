using System.Collections.ObjectModel;
using NeatSplit.Models;

namespace NeatSplit;

public static class AppData
{
    public static ObservableCollection<Group> Groups { get; } = new();
    public static ObservableCollection<Member> Members { get; } = new();
    public static ObservableCollection<Expense> Expenses { get; } = new();
} 