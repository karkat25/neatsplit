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
            System.Diagnostics.Debug.WriteLine($"=== DATABASE DEBUG INFO ===");
            System.Diagnostics.Debug.WriteLine($"Database path: {dbPath}");
            System.Diagnostics.Debug.WriteLine($"AppDataDirectory: {FileSystem.AppDataDirectory}");
            
            _database = new SQLiteAsyncConnection(dbPath);
            System.Diagnostics.Debug.WriteLine("SQLite connection created");
            
            // Create tables
            System.Diagnostics.Debug.WriteLine("Creating database tables...");
            _database.CreateTableAsync<Group>().Wait();
            _database.CreateTableAsync<Member>().Wait();
            _database.CreateTableAsync<Expense>().Wait();
            _database.CreateTableAsync<PaidPayment>().Wait();
            System.Diagnostics.Debug.WriteLine("Database tables created successfully");
            
            // Test the connection
            System.Diagnostics.Debug.WriteLine("Testing database connection...");
            var testResult = _database.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Groups").Result;
            System.Diagnostics.Debug.WriteLine($"Database test result: {testResult} groups in table");
            System.Diagnostics.Debug.WriteLine($"=== END DATABASE DEBUG INFO ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DatabaseService constructor error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
    
    // Group operations
    public async Task<List<Group>> GetGroupsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== GET GROUPS DEBUG ===");
            System.Diagnostics.Debug.WriteLine($"GetGroupsAsync: Starting to retrieve groups...");
            System.Diagnostics.Debug.WriteLine($"Database connection state: {_database != null}");
            
            if (_database == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Database connection is null!");
                return new List<Group>();
            }
            
            var groups = await _database.Table<Group>().ToListAsync();
            System.Diagnostics.Debug.WriteLine($"GetGroupsAsync: Found {groups.Count} groups");
            foreach (var group in groups)
            {
                System.Diagnostics.Debug.WriteLine($"  - Group: '{group.Name}' (ID: {group.Id}, Description: '{group.Description}', Created: {group.CreatedDate})");
            }
            System.Diagnostics.Debug.WriteLine($"=== END GET GROUPS DEBUG ===");
            return groups;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetGroupsAsync error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new List<Group>();
        }
    }
    
    public async Task<int> SaveGroupAsync(Group group)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== SAVE GROUP DEBUG ===");
            System.Diagnostics.Debug.WriteLine($"SaveGroupAsync: Saving group '{group.Name}' (ID: {group.Id}, Description: '{group.Description}', Created: {group.CreatedDate})");
            System.Diagnostics.Debug.WriteLine($"Database connection state: {_database != null}");
            
            if (_database == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Database connection is null!");
                throw new InvalidOperationException("Database connection is not initialized");
            }
            
            int result;
            if (group.Id != 0)
            {
                System.Diagnostics.Debug.WriteLine($"Updating existing group with ID {group.Id}...");
                result = await _database.UpdateAsync(group);
                System.Diagnostics.Debug.WriteLine($"Updated group {group.Name} with ID {group.Id}, result: {result}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Inserting new group...");
                result = await _database.InsertAsync(group);
                System.Diagnostics.Debug.WriteLine($"Inserted group {group.Name} with ID {result}");
                System.Diagnostics.Debug.WriteLine($"Group ID after insert: {group.Id}");
            }
            
            // Verify the group was actually saved
            var savedGroups = await _database.Table<Group>().ToListAsync();
            System.Diagnostics.Debug.WriteLine($"Total groups in database after save: {savedGroups.Count}");
            foreach (var savedGroup in savedGroups)
            {
                System.Diagnostics.Debug.WriteLine($"  - Saved group: '{savedGroup.Name}' (ID: {savedGroup.Id})");
            }
            System.Diagnostics.Debug.WriteLine($"=== END SAVE GROUP DEBUG ===");
            
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveGroupAsync error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
                System.Diagnostics.Debug.WriteLine("ERROR: Database connection is null!");
                return new List<Member>();
            }
            var members = await _database.Table<Member>().ToListAsync();
            System.Diagnostics.Debug.WriteLine($"GetMembersAsync: Found {members.Count} members");
            return members;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetMembersAsync error: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"Updated member {member.Name} with ID {member.Id}");
            }
            else
            {
                result = await _database.InsertAsync(member);
                System.Diagnostics.Debug.WriteLine($"Inserted member {member.Name} with ID {result}");
            }
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveMemberAsync error: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine("ERROR: Database connection is null!");
                return new List<Expense>();
            }
            var expenses = await _database.Table<Expense>().ToListAsync();
            System.Diagnostics.Debug.WriteLine($"GetExpensesAsync: Found {expenses.Count} expenses");
            return expenses;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetExpensesAsync error: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"Updated expense {expense.Description} with ID {expense.Id}");
            }
            else
            {
                result = await _database.InsertAsync(expense);
                System.Diagnostics.Debug.WriteLine($"Inserted expense {expense.Description} with ID {result}");
            }
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveExpenseAsync error: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine("ERROR: Database connection is null!");
            return new List<PaidPayment>();
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
    
    // Clear all paid payments for a group
    public async Task ClearPaidPaymentsForGroupAsync(int groupId)
    {
        if (_database == null)
        {
            throw new InvalidOperationException("Database connection is not initialized");
        }
        await _database.ExecuteAsync("DELETE FROM PaidPayment WHERE GroupId = ?", groupId);
    }

    // Clear all data from all tables
    public async Task ClearAllDataAsync()
    {
        try
        {
            if (_database == null)
            {
                throw new InvalidOperationException("Database connection is not initialized");
            }
            System.Diagnostics.Debug.WriteLine("DatabaseService.ClearAllDataAsync: Clearing all tables...");
            await _database.DeleteAllAsync<Group>();
            await _database.DeleteAllAsync<Member>();
            await _database.DeleteAllAsync<Expense>();
            await _database.DeleteAllAsync<PaidPayment>();
            System.Diagnostics.Debug.WriteLine("DatabaseService.ClearAllDataAsync: All tables cleared successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DatabaseService.ClearAllDataAsync error: {ex.Message}");
            throw;
        }
    }
} 