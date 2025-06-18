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
        /// </summary>
        private async Task InitializeDatabaseAsync()
        {
            await _database.CreateTableAsync<Group>();
            await _database.CreateTableAsync<Member>();
            await _database.CreateTableAsync<Expense>();
            await _database.CreateTableAsync<ExpenseItem>();
            await _database.CreateTableAsync<ExpenseItemParticipant>();
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

        public async Task<int> DeleteAsync<T>(int id) where T : new(), new()
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
        public async Task<int> SaveGroupAsync(Group group)
        {
            if (group.Id != 0)
                return await UpdateAsync(group);
            else
                return await InsertAsync(group);
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
                return await InsertAsync(member);
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
        public async Task<int> SaveExpenseAsync(Expense expense)
        {
            if (expense.Id != 0)
                return await UpdateAsync(expense);
            else
                return await InsertAsync(expense);
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
        public async Task<int> SaveExpenseItemAsync(ExpenseItem expenseItem)
        {
            if (expenseItem.Id != 0)
                return await UpdateAsync(expenseItem);
            else
                return await InsertAsync(expenseItem);
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
        public async Task<int> SaveExpenseItemParticipantAsync(ExpenseItemParticipant participant)
            => await _database.InsertAsync(participant);
        public async Task<int> DeleteExpenseItemParticipantAsync(int expenseItemId, int memberId)
        {
            var participant = await _database.Table<ExpenseItemParticipant>()
                .Where(eip => eip.ExpenseItemId == expenseItemId && eip.MemberId == memberId)
                .FirstOrDefaultAsync();
            if (participant != null)
                return await _database.DeleteAsync(participant);
            return 0;
        }
    }
} 