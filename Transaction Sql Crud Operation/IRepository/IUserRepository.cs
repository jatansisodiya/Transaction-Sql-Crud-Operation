using Transaction_Sql_Crud_Operation.Models;

namespace Transaction_Sql_Crud_Operation.Repositories;

public interface IUserRepository
{
    // Case 1: Scalar
    Task<int> GetUserCountAsync();

    // Case 2: Single entity
    Task<User> GetUserByIdAsync(string userId);

    // Case 3: List
    Task<List<User>> GetAllUsersAsync();

    // Case 4: Specific result set
    Task<List<User>> GetActiveUsersAsync();

    // Case 5: Two result sets
    Task<(List<User> Users, UserSummary Summary)> GetUsersWithSummaryAsync();

    // Case 6: Three result sets
    Task<(List<User> ActiveUsers, List<User> InactiveUsers, UserSummary Summary)> GetUsersGroupedAsync();

    // Case 7: Multiple SPs in single transaction with rollback on failure
    Task<(int CreatedId, User? CreatedUser)> CreateUserWithDetailsAsync(User user, string additionalInfo);

    // Case 8: Call two SPs from separate methods in shared transaction
    Task<(int UpdatedCount, List<User> UpdatedUsers)> UpdateAndFetchUsersAsync(string criteria);

    // Helper methods for Case 8
    Task<int> UpdateUsersByCriteriaAsync(string criteria);
    Task<List<User>> FetchUpdatedUsersAsync(string criteria);
}