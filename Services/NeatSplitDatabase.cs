using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NeatSplit.Models;

namespace NeatSplit.Services
{
    /// <summary>
    /// Manages all database interactions for the NeatSplit application using SQLite.
    /// </summary>
    public class NeatSplitDatabase
    {
        private SQLiteAsyncConnection _database;

        public NeatSplitDatabase()
        {
            // The database path should be specific to the platform.
            // On Android/iOS, this is typically ApplicationDataDirectory.
            // On Windows, it could be LocalApplicationData.
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "NeatSplit.db3");

            _database = new SQLiteAsyncConnection(databasePath);
            InitializeDatabaseAsync().Wait(); // Wait for initialization to complete
        }

        /// <summary>
        /// Initializes the database by creating all necessary tables if they don't exist.
        /// Also creates unique indexes to prevent duplicate entries.
        /// </summary>
        private async Task InitializeDatabaseAsync()
        {
            await _database.CreateTableAsync<Group>();
            await _database.CreateTableAsync<Member>();
            await _database.CreateTableAsync<Expense>();
            await _database.CreateTableAsync<ExpenseItem>();
            await _database.CreateTableAsync<ExpenseItemParticipant>();

            // Create unique indexes to prevent duplicate entries
            await CreateUniqueIndexesAsync();
        }

        /// <summary>
        /// Creates unique indexes to prevent duplicate entries and ensure data integrity.
        /// </summary>
        private async Task CreateUniqueIndexesAsync()
        {
            try
            {
                // Prevent duplicate members in the same group with the same name
                await _database.ExecuteAsync(
                    "CREATE UNIQUE INDEX IF NOT EXISTS idx_unique_member_name_per_group " +
                    "ON Members(GroupId, Name);");

                // Prevent duplicate expense item participants (same member participating in same item)
                await _database.ExecuteAsync(
                    "CREATE UNIQUE INDEX IF NOT EXISTS idx_unique_participant " +
                    "ON ExpenseItemParticipants(ExpenseItemId, MemberId);");

                // Prevent duplicate expenses with same description, amount, and payer in the same group on the same date
                await _database.ExecuteAsync(
                    "CREATE UNIQUE INDEX IF NOT EXISTS idx_unique_expense " +
                    "ON Expenses(GroupId, Description, TotalAmount, PaidByMemberId, ExpenseDate);");

                // Prevent duplicate expense items with same description and amount in the same expense
                await _database.ExecuteAsync(
                    "CREATE UNIQUE INDEX IF NOT EXISTS idx_unique_expense_item " +
                    "ON ExpenseItems(ExpenseId, Description, Amount);");
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the initialization
                System.Diagnostics.Debug.WriteLine($"Error creating unique indexes: {ex.Message}");
            }
        }

        // --- Generic CRUD Operations ---
        public async Task<int> InsertAsync<T>(T entity) where T : new()
        {
            return await _database.InsertAsync(entity);
        }

        public async Task<int> UpdateAsync<T>(T entity) where T : new()
        {
            return await _database.UpdateAsync(entity);
        }

        public async Task<int> DeleteAsync<T>(int id) where T : new()
        {
            return await _database.DeleteAsync<T>(id);
        }

        public async Task<T> GetByIdAsync<T>(int id) where T : new()
        {
            return await _database.GetAsync<T>(id);
        }

        public async Task<List<T>> GetAllAsync<T>() where T : new()
        {
            return await _database.Table<T>().ToListAsync();
        }

        // --- Specific Methods for NeatSplit Entities ---
        // Group Operations
        public async Task<List<Group>> GetGroupsAsync() => await GetAllAsync<Group>();
        public async Task<Group> GetGroupAsync(int id) => await GetByIdAsync<Group>(id);
        public async Task<int> AddGroupAsync(Group group)
        {
            group.Id = 0;
            group.CreatedDate = DateTime.Now;
            // Check for duplicate group name in recent 5 minutes
            var recent = await _database.Table<Group>()
                .Where(g => g.Name == group.Name && g.CreatedDate > DateTime.Now.AddMinutes(-5))
                .FirstOrDefaultAsync();
            if (recent != null)
                throw new InvalidOperationException($"A group named '{group.Name}' was just created.");
            return await _database.InsertAsync(group);
        }
        public async Task<int> DeleteGroupAsync(Group group)
        {
            var members = await GetMembersForGroupAsync(group.Id);
            foreach (var member in members)
                await DeleteMemberAsync(member);
            var expenses = await GetExpensesForGroupAsync(group.Id);
            foreach (var expense in expenses)
                await DeleteExpenseAsync(expense);
            return await DeleteAsync<Group>(group.Id);
        }
        // Member Operations
        public async Task<List<Member>> GetMembersForGroupAsync(int groupId)
            => await _database.Table<Member>().Where(m => m.GroupId == groupId).ToListAsync();
        public async Task<Member> GetMemberAsync(int id) => await GetByIdAsync<Member>(id);
        public async Task<int> SaveMemberAsync(Member member)
        {
            if (member.Id != 0)
                return await UpdateAsync(member);
            else
            {
                // Check for existing member with same name in the same group (recent 5 min)
                var existingMember = await _database.Table<Member>()
                    .Where(m => m.GroupId == member.GroupId && m.Name == member.Name && m.CreatedDate > DateTime.Now.AddMinutes(-5))
                    .FirstOrDefaultAsync();
                if (existingMember != null)
                    throw new InvalidOperationException($"A member with the name '{member.Name}' was just added to this group.");
                return await InsertAsync(member);
            }
        }
        public async Task<int> DeleteMemberAsync(Member member)
        {
            var participants = await _database.Table<ExpenseItemParticipant>().Where(eip => eip.MemberId == member.Id).ToListAsync();
            foreach (var participant in participants)
                await _database.DeleteAsync(participant);
            return await DeleteAsync<Member>(member.Id);
        }
        // Expense Operations
        public async Task<List<Expense>> GetExpensesForGroupAsync(int groupId)
            => await _database.Table<Expense>().Where(e => e.GroupId == groupId).OrderByDescending(e => e.ExpenseDate).ToListAsync();
        public async Task<Expense> GetExpenseAsync(int id) => await GetByIdAsync<Expense>(id);
        public async Task<int> AddExpenseAsync(Expense expense)
        {
            expense.Id = 0;
            expense.CreatedDate = DateTime.Now;
            // Check for duplicate expense in recent 5 min
            var recent = await _database.Table<Expense>()
                .Where(e => e.GroupId == expense.GroupId && e.Description == expense.Description && e.TotalAmount == expense.TotalAmount && e.PaidByMemberId == expense.PaidByMemberId && e.ExpenseDate.Date == expense.ExpenseDate.Date && e.CreatedDate > DateTime.Now.AddMinutes(-5))
                .FirstOrDefaultAsync();
            if (recent != null)
                throw new InvalidOperationException($"A similar expense was just added.");
            return await _database.InsertAsync(expense);
        }
        public async Task<int> DeleteExpenseAsync(Expense expense)
        {
            var items = await GetExpenseItemsForExpenseAsync(expense.Id);
            foreach (var item in items)
                await DeleteExpenseItemAsync(item);
            return await DeleteAsync<Expense>(expense.Id);
        }
        // ExpenseItem Operations
        public async Task<List<ExpenseItem>> GetExpenseItemsForExpenseAsync(int expenseId)
            => await _database.Table<ExpenseItem>().Where(ei => ei.ExpenseId == expenseId).ToListAsync();
        public async Task<ExpenseItem> GetExpenseItemAsync(int id) => await GetByIdAsync<ExpenseItem>(id);
        public async Task<int> AddExpenseItemAsync(ExpenseItem item)
        {
            item.Id = 0;
            item.CreatedDate = DateTime.Now;
            // Check for duplicate item in recent 5 min
            var recent = await _database.Table<ExpenseItem>()
                .Where(ei => ei.ExpenseId == item.ExpenseId && ei.Description == item.Description && ei.Amount == item.Amount && ei.CreatedDate > DateTime.Now.AddMinutes(-5))
                .FirstOrDefaultAsync();
            if (recent != null)
                throw new InvalidOperationException($"A similar expense item was just added.");
            return await _database.InsertAsync(item);
        }
        public async Task<int> DeleteExpenseItemAsync(ExpenseItem expenseItem)
        {
            var participants = await _database.Table<ExpenseItemParticipant>().Where(eip => eip.ExpenseItemId == expenseItem.Id).ToListAsync();
            foreach (var participant in participants)
                await _database.DeleteAsync(participant);
            return await DeleteAsync<ExpenseItem>(expenseItem.Id);
        }
        // ExpenseItemParticipant Operations
        public async Task<List<ExpenseItemParticipant>> GetParticipantsForExpenseItemAsync(int expenseItemId)
            => await _database.Table<ExpenseItemParticipant>().Where(eip => eip.ExpenseItemId == expenseItemId).ToListAsync();
        public async Task<List<ExpenseItemParticipant>> GetExpenseItemParticipantsForMemberAsync(int memberId)
            => await _database.Table<ExpenseItemParticipant>().Where(eip => eip.MemberId == memberId).ToListAsync();
        public async Task<int> AddExpenseItemParticipantAsync(ExpenseItemParticipant participant)
        {
            participant.Id = 0;
            participant.CreatedDate = DateTime.Now;
            // Check for duplicate participant in recent 5 min
            var recent = await _database.Table<ExpenseItemParticipant>()
                .Where(eip => eip.ExpenseItemId == participant.ExpenseItemId && eip.MemberId == participant.MemberId && eip.CreatedDate > DateTime.Now.AddMinutes(-5))
                .FirstOrDefaultAsync();
            if (recent != null)
                throw new InvalidOperationException($"This participant was just added to this item.");
            return await _database.InsertAsync(participant);
        }
        public async Task<int> DeleteExpenseItemParticipantAsync(int expenseItemId, int memberId)
        {
            var participant = await _database.Table<ExpenseItemParticipant>()
                .Where(eip => eip.ExpenseItemId == expenseItemId && eip.MemberId == memberId)
                .FirstOrDefaultAsync();
            if (participant != null)
                return await _database.DeleteAsync(participant);
            return 0;
        }

        /// <summary>
        /// Validates data integrity by checking for orphaned records.
        /// </summary>
        public async Task<List<string>> ValidateDataIntegrityAsync()
        {
            var issues = new List<string>();

            try
            {
                // Check for orphaned expense items (expense doesn't exist)
                var orphanedItems = await _database.QueryAsync<ExpenseItem>(
                    "SELECT ei.* FROM ExpenseItems ei " +
                    "LEFT JOIN Expenses e ON ei.ExpenseId = e.Id " +
                    "WHERE e.Id IS NULL");
                
                if (orphanedItems.Count > 0)
                {
                    issues.Add($"Found {orphanedItems.Count} orphaned expense items");
                }

                // Check for orphaned participants (expense item doesn't exist)
                var orphanedParticipants = await _database.QueryAsync<ExpenseItemParticipant>(
                    "SELECT eip.* FROM ExpenseItemParticipants eip " +
                    "LEFT JOIN ExpenseItems ei ON eip.ExpenseItemId = ei.Id " +
                    "WHERE ei.Id IS NULL");
                
                if (orphanedParticipants.Count > 0)
                {
                    issues.Add($"Found {orphanedParticipants.Count} orphaned expense item participants");
                }

                // Check for orphaned expenses (group doesn't exist)
                var orphanedExpenses = await _database.QueryAsync<Expense>(
                    "SELECT e.* FROM Expenses e " +
                    "LEFT JOIN Groups g ON e.GroupId = g.Id " +
                    "WHERE g.Id IS NULL");
                
                if (orphanedExpenses.Count > 0)
                {
                    issues.Add($"Found {orphanedExpenses.Count} orphaned expenses");
                }

                // Check for orphaned members (group doesn't exist)
                var orphanedMembers = await _database.QueryAsync<Member>(
                    "SELECT m.* FROM Members m " +
                    "LEFT JOIN Groups g ON m.GroupId = g.Id " +
                    "WHERE g.Id IS NULL");
                
                if (orphanedMembers.Count > 0)
                {
                    issues.Add($"Found {orphanedMembers.Count} orphaned members");
                }
            }
            catch (Exception ex)
            {
                issues.Add($"Error during data integrity check: {ex.Message}");
            }

            return issues;
        }

        /// <summary>
        /// Cleans up orphaned records to maintain data integrity.
        /// </summary>
        public async Task<int> CleanupOrphanedRecordsAsync()
        {
            int deletedCount = 0;

            try
            {
                // Delete orphaned expense items
                deletedCount += await _database.ExecuteAsync(
                    "DELETE FROM ExpenseItems WHERE ExpenseId NOT IN (SELECT Id FROM Expenses)");

                // Delete orphaned participants
                deletedCount += await _database.ExecuteAsync(
                    "DELETE FROM ExpenseItemParticipants WHERE ExpenseItemId NOT IN (SELECT Id FROM ExpenseItems)");

                // Delete orphaned expenses
                deletedCount += await _database.ExecuteAsync(
                    "DELETE FROM Expenses WHERE GroupId NOT IN (SELECT Id FROM Groups)");

                // Delete orphaned members
                deletedCount += await _database.ExecuteAsync(
                    "DELETE FROM Members WHERE GroupId NOT IN (SELECT Id FROM Groups)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }

            return deletedCount;
        }
    }
} 