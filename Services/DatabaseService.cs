using SQLite;
using NeatSplit.Models;
using System.Collections.ObjectModel;

namespace NeatSplit.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;
    
    public DatabaseService()
    {
        try
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "neatsplit.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            
            // Create tables
            _database.CreateTableAsync<Group>().Wait();
            _database.CreateTableAsync<Member>().Wait();
            _database.CreateTableAsync<Expense>().Wait();
            _database.CreateTableAsync<PaidPayment>().Wait();
        }
        catch
        {
            throw;
        }
    }
    
    // Group operations
    public async Task<List<Group>> GetGroupsAsync()
    {
        try
        {
            if (_database == null)
            {
                return new List<Group>();
            }
            
            var groups = await _database.Table<Group>().ToListAsync();
            return groups;
        }
        catch
        {
            return new List<Group>();
        }
    }
    
    public async Task<int> SaveGroupAsync(Group group)
    {
        try
        {
            if (_database == null)
            {
                throw new InvalidOperationException("Database connection is not initialized");
            }
            
            int result;
            if (group.Id != 0)
            {
                result = await _database.UpdateAsync(group);
            }
            else
            {
                result = await _database.InsertAsync(group);
            }
            
            return result;
        }
        catch
        {
            throw;
        }
    }
    
    public async Task<int> DeleteGroupAsync(Group group)
    {
        if (_database == null)
        {
            throw new InvalidOperationException("Database connection is not initialized");
        }
        return await _database.DeleteAsync(group);
    }
    
    // Member operations
    public async Task<List<Member>> GetMembersAsync()
    {
        try
        {
            if (_database == null)
            {
                return new List<Member>();
            }
            var members = await _database.Table<Member>().ToListAsync();
            return members;
        }
        catch
        {
            return new List<Member>();
        }
    }
    
    public async Task<int> SaveMemberAsync(Member member)
    {
        try
        {
            if (_database == null)
            {
                throw new InvalidOperationException("Database connection is not initialized");
            }
            int result;
            if (member.Id != 0)
            {
                result = await _database.UpdateAsync(member);
            }
            else
            {
                result = await _database.InsertAsync(member);
            }
            return result;
        }
        catch
        {
            throw;
        }
    }
    
    public async Task<int> DeleteMemberAsync(Member member)
    {
        if (_database == null)
        {
            throw new InvalidOperationException("Database connection is not initialized");
        }
        return await _database.DeleteAsync(member);
    }
    
    // Expense operations
    public async Task<List<Expense>> GetExpensesAsync()
    {
        try
        {
            if (_database == null)
            {
                return new List<Expense>();
            }
            var expenses = await _database.Table<Expense>().ToListAsync();
            return expenses;
        }
        catch
        {
            return new List<Expense>();
        }
    }
    
    public async Task<int> SaveExpenseAsync(Expense expense)
    {
        try
        {
            if (_database == null)
            {
                throw new InvalidOperationException("Database connection is not initialized");
            }
            int result;
            if (expense.Id != 0)
            {
                result = await _database.UpdateAsync(expense);
            }
            else
            {
                result = await _database.InsertAsync(expense);
            }
            return result;
        }
        catch
        {
            throw;
        }
    }
    
    public async Task<int> DeleteExpenseAsync(Expense expense)
    {
        if (_database == null)
        {
            throw new InvalidOperationException("Database connection is not initialized");
        }
        return await _database.DeleteAsync(expense);
    }
    
    // PaidPayment operations
    public async Task<List<PaidPayment>> GetPaidPaymentsAsync()
    {
        if (_database == null)
        {
            throw new InvalidOperationException("Database connection is not initialized");
        }
        return await _database.Table<PaidPayment>().ToListAsync();
    }
    
    public async Task<int> SavePaidPaymentAsync(PaidPayment payment)
    {
        if (_database == null)
        {
            throw new InvalidOperationException("Database connection is not initialized");
        }
        return await _database.InsertAsync(payment);
    }
    
    public async Task<int> DeletePaidPaymentAsync(PaidPayment payment)
    {
        if (_database == null)
        {
            throw new InvalidOperationException("Database connection is not initialized");
        }
        return await _database.DeleteAsync(payment);
    }
    
    public async Task ClearPaidPaymentsForGroupAsync(int groupId)
    {
        if (_database == null)
        {
            throw new InvalidOperationException("Database connection is not initialized");
        }
        await _database.ExecuteAsync("DELETE FROM PaidPayment WHERE GroupId = ?", groupId);
    }
    
    public async Task ClearAllDataAsync()
    {
        try
        {
            await _database.DeleteAllAsync<Group>();
            await _database.DeleteAllAsync<Member>();
            await _database.DeleteAllAsync<Expense>();
            await _database.DeleteAllAsync<PaidPayment>();
        }
        catch
        {
            throw;
        }
    }
} 