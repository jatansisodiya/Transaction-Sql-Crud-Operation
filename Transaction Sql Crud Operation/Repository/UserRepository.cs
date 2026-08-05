using CommonLogger;
using Transaction_Sql_Crud_Operation.Models;
using Transaction.SQLConnection.Interfaces;

namespace Transaction_Sql_Crud_Operation.Repositories;

public class UserRepository(ITransactionalRepositoryAsync repository, IAILogger logger) 
    : IUserRepository
{
    // Case 1: Return scalar value (int)
    public async Task<int> GetUserCountAsync()
    {
        logger.LogInformation("Getting user count");

        return await repository.ExecuteInTransactionAsync<int>(
            "usp_GetAspNetUsersCount",
            isRead: true);
    }

    // Case 2: Return single entity
    public async Task<User?> GetUserByIdAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        logger.LogInformation($"Getting user by ID: {userId}");

        return await repository.ExecuteInTransactionAsync<User>(
            "usp_GetAspNetUserById",
            new { UserId = userId },
            isRead: true);
    }

    // Case 3: Return list of entities
    public async Task<List<User>> GetAllUsersAsync()
    {
        logger.LogInformation("Getting all users");

        return await repository.ExecuteInTransactionAsync<List<User>>(
            "usp_GetAllAspNetUsers",
            isRead: true);
    }

    // Case 4: Return specific result set by index
    public async Task<List<User>> GetActiveUsersAsync()
    {
        logger.LogInformation("Getting active users from result set index 0");

        return await repository.ExecuteInTransactionAsync<List<User>>(
            "usp_GetAllAspNetUsersMultipleResults",
            resultSetIndex: 0,
            isRead: true);
    }

    // Case 5: Return two result sets as tuple
    public async Task<(List<User> Users, UserSummary Summary)> GetUsersWithSummaryAsync()
    {
        logger.LogInformation("Getting users with summary");

        return await repository.ExecuteMultipleResultSetsAsync<List<User>, UserSummary>(
            "usp_GetAspNetUsersWithSummary",
            isRead: true);
    }

    // Case 6: Return three result sets as tuple
    public async Task<(List<User> ActiveUsers, List<User> InactiveUsers, UserSummary Summary)> GetUsersGroupedAsync()
    {
        logger.LogInformation("Getting users grouped with summary");

        return await repository.ExecuteMultipleResultSetsAsync<List<User>, List<User>, UserSummary>(
            "usp_GetAspNetUsersGroupedWithSummary",
            isRead: true);
    }

    // Case 7: Multiple SPs in single transaction - if any fails, rollback all
    public async Task<(int CreatedId, User? CreatedUser)> CreateUserWithDetailsAsync(User user, string additionalInfo)
    {
        logger.LogInformation("Creating user with details in single transaction");

        try
        {
            await repository.BeginTransactionAsync();

            // SP 1: Create user and get new ID
            var createdId = await repository.ExecuteInTransactionAsync<int>(
                "usp_CreateAspNetUser",
                new
                {
                    user.UserName,
                    user.Email,
                    user.PhoneNumber
                });

            // SP 2: Add additional user details
            await repository.ExecuteInTransactionAsync<int>(
                "usp_AddUserDetails",
                new
                {
                    UserId = createdId.ToString(),
                    AdditionalInfo = additionalInfo
                });

            // SP 3: Fetch created user to return
            var createdUser = await repository.ExecuteInTransactionAsync<User>(
                "usp_GetAspNetUserById",
                new { UserId = createdId.ToString() });

            await repository.CommitAsync();

            logger.LogInformation($"User created successfully with ID: {createdId}");
            return (createdId, createdUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create user with details. Rolling back transaction");

            if (repository.HasActiveTransaction)
            {
                await repository.RollbackAsync();
            }

            throw;
        }
    }

    // Case 8: Call two SPs from separate methods in shared transaction
    public async Task<(int UpdatedCount, List<User> UpdatedUsers)> UpdateAndFetchUsersAsync(string criteria)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(criteria);

        logger.LogInformation($"Updating and fetching users with criteria: {criteria}");

        try
        {
            await repository.BeginTransactionAsync();

            var updatedCount = await UpdateUsersByCriteriaAsync(criteria);
            var updatedUsers = await FetchUpdatedUsersAsync(criteria);

            await repository.CommitAsync();

            logger.LogInformation($"Updated {updatedCount} users successfully");
            return (updatedCount, updatedUsers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update and fetch users. Rolling back transaction");

            if (repository.HasActiveTransaction)
            {
                await repository.RollbackAsync();
            }

            throw;
        }
    }

    public async Task<int> UpdateUsersByCriteriaAsync(string criteria)
    {
        logger.LogInformation($"Updating users by criteria: {criteria}");

        return await repository.ExecuteInTransactionAsync<int>(
            "usp_UpdateUsersByCriteria",
            new { Criteria = criteria });
    }

    public async Task<List<User>> FetchUpdatedUsersAsync(string criteria)
    {
        logger.LogInformation($"Fetching updated users by criteria: {criteria}");

        return await repository.ExecuteInTransactionAsync<List<User>>(
            "usp_GetUsersByCriteria",
            new { Criteria = criteria },
            isRead: true);
    }
}