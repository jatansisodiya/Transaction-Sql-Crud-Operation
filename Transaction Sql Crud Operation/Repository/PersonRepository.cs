using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using Transaction_Sql_Crud_Operation.Models;
using Transaction.SQLConnection.Interfaces;

namespace Transaction_Sql_Crud_Operation.Repositories;

public class PersonRepository(ITransactionalRepositoryAsync repository, ILogger<PersonRepository> logger)
    : IPersonRepository
{
    // Get all persons with qualifications (two result sets)
    public async Task<(List<Person> Persons, List<Qualification> Qualifications)> GetAllAsync()
    {
        logger.LogInformation("Getting all persons with qualifications");

        return await repository.ExecuteMultipleResultSetsAsync<List<Person>, List<Qualification>>(
            "usp_Person_GetAll",
            []);
    }

    // Get person by ID with qualifications (two result sets)
    public async Task<(Person? Person, List<Qualification> Qualifications)> GetByIdAsync(int personId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(personId);

        logger.LogInformation("Getting person by ID: {PersonId}", personId);

        return await repository.ExecuteMultipleResultSetsAsync<Person?, List<Qualification>>(
            "usp_Person_GetById",
            [new SqlParameter("@PersonId", personId)]);
    }

    // Create person with qualifications (transactional - multiple SPs)
    public async Task<int> CreateAsync(PersonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.MobileNo);

        logger.LogInformation("Creating person: {Name}", request.Name);

        try
        {
            await repository.BeginTransactionAsync();

            // Insert person with output parameter
            var personIdParam = new SqlParameter("@PersonId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            await repository.ExecuteInTransactionAsync<int>(
                "usp_Person_Insert",
                [
                    new SqlParameter("@Name", request.Name),
                    new SqlParameter("@MobileNo", request.MobileNo),
                    new SqlParameter("@Age", request.Age),
                    new SqlParameter("@Address", request.Address ?? (object)DBNull.Value),
                    personIdParam
                ]);

            var personId = (int)personIdParam.Value;

            // Insert qualifications if any
            if (request.Qualifications?.Count > 0)
            {
                foreach (var qualification in request.Qualifications)
                {
                    await AddQualificationInternalAsync(personId, qualification);
                }
            }

            await repository.CommitAsync();

            logger.LogInformation("Person created with ID: {PersonId}", personId);
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

        logger.LogInformation("Updating person: {PersonId}", personId);

        try
        {
            await repository.BeginTransactionAsync();

            // Update person
            var rowsAffected = await repository.ExecuteInTransactionAsync<int>(
                "usp_Person_Update",
                [
                    new SqlParameter("@PersonId", personId),
                    new SqlParameter("@Name", request.Name),
                    new SqlParameter("@MobileNo", request.MobileNo),
                    new SqlParameter("@Age", request.Age),
                    new SqlParameter("@Address", request.Address ?? (object)DBNull.Value)
                ]);

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
                    [new SqlParameter("@PersonId", personId)]);

                // Insert new qualifications
                foreach (var qualification in request.Qualifications)
                {
                    await AddQualificationInternalAsync(personId, qualification);
                }
            }

            await repository.CommitAsync();

            logger.LogInformation("Person updated: {PersonId}", personId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating person {PersonId}, rolling back", personId);
            await repository.RollbackAsync();
            throw;
        }
    }

    // Delete person
    public async Task<bool> DeleteAsync(int personId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(personId);

        logger.LogInformation("Deleting person: {PersonId}", personId);

        var rowsAffected = await repository.ExecuteInTransactionAsync<int>(
            "usp_Person_Delete",
            [new SqlParameter("@PersonId", personId)]);

        return rowsAffected > 0;
    }

    // Add single qualification
    public async Task<int> AddQualificationAsync(int personId, QualificationRequest request)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(personId);
        ArgumentNullException.ThrowIfNull(request);

        logger.LogInformation("Adding qualification for person: {PersonId}", personId);

        return await AddQualificationInternalAsync(personId, request);
    }

    // Delete single qualification
    public async Task<bool> DeleteQualificationAsync(int qualificationId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(qualificationId);

        logger.LogInformation("Deleting qualification: {QualificationId}", qualificationId);

        var rowsAffected = await repository.ExecuteInTransactionAsync<int>(
            "usp_Qualification_Delete",
            [new SqlParameter("@QualificationId", qualificationId)]);

        return rowsAffected > 0;
    }

    // Internal helper for adding qualification
    private async Task<int> AddQualificationInternalAsync(int personId, QualificationRequest request)
    {
        var qualificationIdParam = new SqlParameter("@QualificationId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        await repository.ExecuteInTransactionAsync<int>(
            "usp_Qualification_Insert",
            [
                new SqlParameter("@PersonId", personId),
                new SqlParameter("@QualificationName", request.QualificationName),
                new SqlParameter("@Marks", request.Marks),
                qualificationIdParam
            ]);

        return (int)qualificationIdParam.Value;
    }
}