using Microsoft.AspNetCore.Mvc;
using Transaction_Sql_Crud_Operation.Models;
using Transaction_Sql_Crud_Operation.Repositories;

namespace Transaction_Sql_Crud_Operation.Controllers;

[ApiController]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public class PersonController(IPersonRepository personRepository) : ControllerBase
{
    // Get all persons with qualifications
    [HttpGet("details")]
    public async Task<IActionResult> GetAll()
    {
        var (persons, qualifications) = await personRepository.GetAllAsync();

        // Map qualifications to persons
        var result = persons.Select(p => new
        {
            p.PersonId,
            p.Name,
            p.MobileNo,
            p.Age,
            p.Address,
            p.CreatedDate,
            p.ModifiedDate,
            Qualifications = qualifications.Where(q => q.PersonId == p.PersonId).ToList()
        });

        return Ok(result);
    }

    // Get person by ID with qualifications
    [HttpGet("{personId:int}")]
    public async Task<IActionResult> GetById(int personId)
    {
        var (person, qualifications) = await personRepository.GetByIdAsync(personId);

        if (person is null)
            return NotFound(new { Message = $"Person with ID '{personId}' not found" });

        return Ok(new
        {
            person.PersonId,
            person.Name,
            person.MobileNo,
            person.Age,
            person.Address,
            person.CreatedDate,
            person.ModifiedDate,
            Qualifications = qualifications
        });
    }
    
    // Create person with qualifications
    [HttpPost("save")]
    public async Task<IActionResult> Create([FromBody] PersonRequest request)
    {
        var personId = await personRepository.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { personId },
            new { PersonId = personId, Message = "Person created successfully" });
    }

    // Update person with qualifications
    [HttpPut("{personId:int}")]
    public async Task<IActionResult> Update(int personId, [FromBody] PersonRequest request)
    {
        var updated = await personRepository.UpdateAsync(personId, request);

        if (!updated)
            return NotFound(new { Message = $"Person with ID '{personId}' not found" });

        return Ok(new { PersonId = personId, Message = "Person updated successfully" });
    }

    // Delete person
    [HttpDelete("{personId:int}")]
    public async Task<IActionResult> Delete(int personId)
    {
        var deleted = await personRepository.DeleteAsync(personId);

        if (!deleted)
            return NotFound(new { Message = $"Person with ID '{personId}' not found" });

        return Ok(new { PersonId = personId, Message = "Person deleted successfully" });
    }

    // Add qualification to person
    [HttpPost("{personId:int}/qualifications")]
    public async Task<IActionResult> AddQualification(int personId, [FromBody] QualificationRequest request)
    {
        var qualificationId = await personRepository.AddQualificationAsync(personId, request);

        return CreatedAtAction(
            nameof(GetById),
            new { personId },
            new { QualificationId = qualificationId, Message = "Qualification added successfully" });
    }

    // Delete qualification
    [HttpDelete("qualifications/{qualificationId:int}")]
    public async Task<IActionResult> DeleteQualification(int qualificationId)
    {
        var deleted = await personRepository.DeleteQualificationAsync(qualificationId);

        if (!deleted)
            return NotFound(new { Message = $"Qualification with ID '{qualificationId}' not found" });

        return Ok(new { QualificationId = qualificationId, Message = "Qualification deleted successfully" });
    }
}