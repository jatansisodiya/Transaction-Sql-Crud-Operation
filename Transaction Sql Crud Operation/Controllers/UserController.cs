using Microsoft.AspNetCore.Mvc;
using Transaction_Sql_Crud_Operation.Models;
using Transaction_Sql_Crud_Operation.Repositories;

namespace Transaction_Sql_Crud_Operation.Controllers;

[ApiController]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public class UserController(IUserRepository userRepository) : ControllerBase
{
    // Case 1: Scalar value
    [HttpGet("count")]
    public async Task<IActionResult> GetUserCount()
    {
        var count = await userRepository.GetUserCountAsync();
        return Ok(new { TotalUsers = count });
    }

    // Case 2: Single entity
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        var user = await userRepository.GetUserByIdAsync(userId);

        return user is null
            ? NotFound(new { Message = $"User with ID '{userId}' not found" })
            : Ok(user);
    }

    // Case 3: List of entities
    [HttpGet("details")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await userRepository.GetAllUsersAsync();
        return Ok(users);
    }

    // Case 4: Specific result set by index
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveUsers()
    {
        var activeUsers = await userRepository.GetActiveUsersAsync();
        return Ok(activeUsers);
    }

    // Case 5: Two result sets
    [HttpGet("with-summary")]
    public async Task<IActionResult> GetUsersWithSummary()
    {
        var (users, summary) = await userRepository.GetUsersWithSummaryAsync();
        return Ok(new { Users = users, Summary = summary });
    }

    // Case 6: Three result sets
    [HttpGet("grouped")]
    public async Task<IActionResult> GetUsersGrouped()
    {
        var (activeUsers, inactiveUsers, summary) = await userRepository.GetUsersGroupedAsync();
        return Ok(new { ActiveUsers = activeUsers, InactiveUsers = inactiveUsers, Summary = summary });
    }

    // Case 7: Multiple SPs in single transaction with rollback on failure
    [HttpPost("with-details")]
    public async Task<IActionResult> CreateUserWithDetails([FromBody] CreateUserRequest request)
    {
        var (createdId, createdUser) = await userRepository.CreateUserWithDetailsAsync(
            request.User,
            request.AdditionalInfo);

        return CreatedAtAction(
            nameof(GetUserById),
            new { userId = createdId.ToString() },
            new { Id = createdId, User = createdUser });
    }

    // Case 8: Call two SPs from separate methods in shared transaction
    [HttpPut("batch-update")]
    public async Task<IActionResult> UpdateAndFetchUsers([FromQuery] string criteria)
    {
        var (updatedCount, updatedUsers) = await userRepository.UpdateAndFetchUsersAsync(criteria);

        return Ok(new
        {
            UpdatedCount = updatedCount,
            UpdatedUsers = updatedUsers
        });
    }
}