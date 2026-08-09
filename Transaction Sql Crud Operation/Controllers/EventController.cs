using Microsoft.AspNetCore.Mvc;

namespace Transaction_Sql_Crud_Operation.Controllers;

[ApiController]
[Route("api/v1/events")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status202Accepted)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public class EventController : ControllerBase
{
    private readonly ILogger<EventController> _logger;

    /// <inheritdoc cref="EventController"/>
    public EventController(ILogger<EventController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retrieves events (stub endpoint).
    /// </summary>
    [HttpGet]
    public Task<IActionResult> GetEvents()
    {
        return Task.FromResult<IActionResult>(Ok(new { message = "Events retrieved successfully." }));
    }
}
