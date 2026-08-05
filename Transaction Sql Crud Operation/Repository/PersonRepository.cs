using CommonLogger;
using Dapper;
using System.Data;
using Transaction_Sql_Crud_Operation.Models;
using Transaction.SQLConnection.Interfaces;

namespace Transaction_Sql_Crud_Operation.Repositories;

public class PersonRepository(ITransactionalRepositoryAsync repository, IAILogger logger)
    : IPersonRepository
{
    // Get all persons with qualifications (two result sets)
    public async Task<(List<Person> Persons, List<Qualification> Qualifications)> GetAllAsync()
    {
        logger.LogInformation("Getting all persons with qualifications");

        return await repository.ExecuteMultipleResultSetsAsync<List<Person>, List<Qualification>>(
            "usp_Person_GetAll",
            isRead: true);
    }

    // Get person by ID with qualifications (two result sets)
    public async Task<(Person? Person, List<Qualification> Qualifications)> GetByIdAsync(int personId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(personId);

        logger.LogInformation($"Getting person by ID: {personId}");

        return await repository.ExecuteMultipleResultSetsAsync<Person?, List<Qualification>>(
            "usp_Person_GetById",
            new { PersonId = personId },
            isRead: true);
    }

    // Create person with qualifications (transactional - multiple SPs)
    public async Task<int> CreateAsync(PersonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.MobileNo);

        logger.LogInformation($"Creating person: {request.Name}");

        try
        {
            await repository.BeginTransactionAsync();

            var p = new DynamicParameters();
            p.Add("@Name", request.Name);
            p.Add("@MobileNo", request.MobileNo);
            p.Add("@Age", request.Age);
            p.Add("@Address", request.Address);
            p.Add("@PersonId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await repository.ExecuteInTransactionAsync<int>("usp_Person_Insert", p);

            var personId = p.Get<int>("@PersonId");

            // Insert qualifications if any
            if (request.Qualifications?.Count > 0)
            {
                foreach (var qualification in request.Qualifications)
                {
                    await AddQualificationInternalAsync(personId, qualification);
                }
            }

            await repository.CommitAsync();

            logger.LogInformation($"Person created with ID: {personId}");
            return personId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating person, rolling back transaction");
            await repository.RollbackAsync();
            throw;
        }
    }

    // Update person with qualifications (transactional)
    public async Task<bool> UpdateAsync(int personId, PersonRequest request)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(personId);
        ArgumentNullException.ThrowIfNull(request);

        logger.LogInformation($"Updating person: {personId}");

        try
        {
            await repository.BeginTransactionAsync();

            // Update person
            var rowsAffected = await repository.ExecuteInTransactionAsync<int>(
                "usp_Person_Update",
                new
                {
                    PersonId = personId,
                    request.Name,
                    request.MobileNo,
                    request.Age,
                    request.Address
                });

            if (rowsAffected == 0)
            {
                await repository.RollbackAsync();
                return false;
            }

            // Replace qualifications if provided
            if (request.Qualifications != null)
            {
                // Delete existing qualifications
                await repository.ExecuteInTransactionAsync<int>(
                    "usp_Qualification_DeleteByPersonId",
                    new { PersonId = personId });

                // Insert new qualifications
                foreach (var qualification in request.Qualifications)
                {
                    await AddQualificationInternalAsync(personId, qualification);
                }
            }

            await repository.CommitAsync();

            logger.LogInformation($"Person updated: {personId}");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error updating person {personId}, rolling back");
            await repository.RollbackAsync();
            throw;
        }
    }

    // Delete person
    public async Task<bool> DeleteAsync(int personId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(personId);

        logger.LogInformation($"Deleting person: {personId}");

        var rowsAffected = await repository.ExecuteInTransactionAsync<int>(
            "usp_Person_Delete",
            new { PersonId = personId });

        return rowsAffected > 0;
    }

    // Add single qualification
    public async Task<int> AddQualificationAsync(int personId, QualificationRequest request)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(personId);
        ArgumentNullException.ThrowIfNull(request);

        logger.LogInformation($"Adding qualification for person: {personId}");

        return await AddQualificationInternalAsync(personId, request);
    }

    // Delete single qualification
    public async Task<bool> DeleteQualificationAsync(int qualificationId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(qualificationId);

        logger.LogInformation($"Deleting qualification: {qualificationId}");

        var rowsAffected = await repository.ExecuteInTransactionAsync<int>(
            "usp_Qualification_Delete",
            new { QualificationId = qualificationId });

        return rowsAffected > 0;
    }

    // Internal helper for adding qualification
    private async Task<int> AddQualificationInternalAsync(int personId, QualificationRequest request)
    {
        var p = new DynamicParameters();
        p.Add("@PersonId", personId);
        p.Add("@QualificationName", request.QualificationName);
        p.Add("@Marks", request.Marks);
        p.Add("@QualificationId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await repository.ExecuteInTransactionAsync<int>("usp_Qualification_Insert", p);

        return p.Get<int>("@QualificationId");
    }
}