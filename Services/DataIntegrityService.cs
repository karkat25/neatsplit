using NeatSplit.Models;

namespace NeatSplit.Services
{
    /// <summary>
    /// Service for maintaining data integrity and preventing duplicate entries.
    /// </summary>
    public class DataIntegrityService
    {
        private readonly NeatSplitDatabase _database;

        public DataIntegrityService(NeatSplitDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Performs a comprehensive data integrity check and returns any issues found.
        /// </summary>
        public async Task<List<string>> PerformIntegrityCheckAsync()
        {
            var issues = new List<string>();

            try
            {
                // Check for data integrity issues
                var integrityIssues = await _database.ValidateDataIntegrityAsync();
                issues.AddRange(integrityIssues);

                // Check for potential duplicate entries
                var duplicateIssues = await CheckForPotentialDuplicatesAsync();
                issues.AddRange(duplicateIssues);

                // Check for data consistency issues
                var consistencyIssues = await CheckDataConsistencyAsync();
                issues.AddRange(consistencyIssues);
            }
            catch (Exception ex)
            {
                issues.Add($"Error during integrity check: {ex.Message}");
            }

            return issues;
        }

        /// <summary>
        /// Checks for potential duplicate entries that might have slipped through.
        /// </summary>
        private async Task<List<string>> CheckForPotentialDuplicatesAsync()
        {
            var issues = new List<string>();

            try
            {
                // Check for duplicate member names in the same group
                var duplicateMembers = await _database.QueryAsync<Member>(
                    "SELECT GroupId, Name, COUNT(*) as Count " +
                    "FROM Members " +
                    "GROUP BY GroupId, Name " +
                    "HAVING COUNT(*) > 1");

                if (duplicateMembers.Count > 0)
                {
                    issues.Add($"Found {duplicateMembers.Count} groups with duplicate member names");
                }

                // Check for duplicate expense item participants
                var duplicateParticipants = await _database.QueryAsync<ExpenseItemParticipant>(
                    "SELECT ExpenseItemId, MemberId, COUNT(*) as Count " +
                    "FROM ExpenseItemParticipants " +
                    "GROUP BY ExpenseItemId, MemberId " +
                    "HAVING COUNT(*) > 1");

                if (duplicateParticipants.Count > 0)
                {
                    issues.Add($"Found {duplicateParticipants.Count} expense items with duplicate participants");
                }
            }
            catch (Exception ex)
            {
                issues.Add($"Error checking for duplicates: {ex.Message}");
            }

            return issues;
        }

        /// <summary>
        /// Checks for data consistency issues.
        /// </summary>
        private async Task<List<string>> CheckDataConsistencyAsync()
        {
            var issues = new List<string>();

            try
            {
                // Check for expenses with invalid member references
                var invalidExpenses = await _database.QueryAsync<Expense>(
                    "SELECT e.* FROM Expenses e " +
                    "LEFT JOIN Members m ON e.PaidByMemberId = m.Id " +
                    "WHERE m.Id IS NULL");

                if (invalidExpenses.Count > 0)
                {
                    issues.Add($"Found {invalidExpenses.Count} expenses with invalid payer references");
                }

                // Check for expense items with invalid expense references
                var invalidItems = await _database.QueryAsync<ExpenseItem>(
                    "SELECT ei.* FROM ExpenseItems ei " +
                    "LEFT JOIN Expenses e ON ei.ExpenseId = e.Id " +
                    "WHERE e.Id IS NULL");

                if (invalidItems.Count > 0)
                {
                    issues.Add($"Found {invalidItems.Count} expense items with invalid expense references");
                }

                // Check for participants with invalid member references
                var invalidParticipants = await _database.QueryAsync<ExpenseItemParticipant>(
                    "SELECT eip.* FROM ExpenseItemParticipants eip " +
                    "LEFT JOIN Members m ON eip.MemberId = m.Id " +
                    "WHERE m.Id IS NULL");

                if (invalidParticipants.Count > 0)
                {
                    issues.Add($"Found {invalidParticipants.Count} participants with invalid member references");
                }
            }
            catch (Exception ex)
            {
                issues.Add($"Error checking data consistency: {ex.Message}");
            }

            return issues;
        }

        /// <summary>
        /// Performs cleanup operations to fix data integrity issues.
        /// </summary>
        public async Task<(int deletedCount, List<string> issues)> PerformCleanupAsync()
        {
            var issues = new List<string>();
            int deletedCount = 0;

            try
            {
                // Clean up orphaned records
                deletedCount = await _database.CleanupOrphanedRecordsAsync();

                // Remove duplicate participants (keep the first one)
                var duplicateParticipantsDeleted = await _database.ExecuteAsync(@"
                    DELETE FROM ExpenseItemParticipants 
                    WHERE Id NOT IN (
                        SELECT MIN(Id) 
                        FROM ExpenseItemParticipants 
                        GROUP BY ExpenseItemId, MemberId
                    )");

                deletedCount += duplicateParticipantsDeleted;

                if (duplicateParticipantsDeleted > 0)
                {
                    issues.Add($"Removed {duplicateParticipantsDeleted} duplicate participants");
                }

                // Remove duplicate members (keep the first one)
                var duplicateMembersDeleted = await _database.ExecuteAsync(@"
                    DELETE FROM Members 
                    WHERE Id NOT IN (
                        SELECT MIN(Id) 
                        FROM Members 
                        GROUP BY GroupId, Name
                    )");

                deletedCount += duplicateMembersDeleted;

                if (duplicateMembersDeleted > 0)
                {
                    issues.Add($"Removed {duplicateMembersDeleted} duplicate members");
                }
            }
            catch (Exception ex)
            {
                issues.Add($"Error during cleanup: {ex.Message}");
            }

            return (deletedCount, issues);
        }

        /// <summary>
        /// Validates that a member name is unique within a group.
        /// </summary>
        public async Task<bool> IsMemberNameUniqueAsync(int groupId, string name, int excludeMemberId = 0)
        {
            try
            {
                var existingMember = await _database.QueryAsync<Member>(
                    "SELECT * FROM Members WHERE GroupId = ? AND Name = ? AND Id != ?",
                    groupId, name, excludeMemberId);

                return existingMember.Count == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that an expense is unique within a group for the same date.
        /// </summary>
        public async Task<bool> IsExpenseUniqueAsync(int groupId, string description, decimal amount, int paidByMemberId, DateTime expenseDate, int excludeExpenseId = 0)
        {
            try
            {
                var existingExpense = await _database.QueryAsync<Expense>(
                    "SELECT * FROM Expenses WHERE GroupId = ? AND Description = ? AND TotalAmount = ? AND PaidByMemberId = ? AND date(ExpenseDate) = date(?) AND Id != ?",
                    groupId, description, amount, paidByMemberId, expenseDate, excludeExpenseId);

                return existingExpense.Count == 0;
            }
            catch
            {
                return false;
            }
        }
    }
} 