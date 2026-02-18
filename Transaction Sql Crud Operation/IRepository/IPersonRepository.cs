using Transaction_Sql_Crud_Operation.Models;

namespace Transaction_Sql_Crud_Operation.Repositories;

public interface IPersonRepository
{
    // Get all persons with qualifications
    Task<(List<Person> Persons, List<Qualification> Qualifications)> GetAllAsync();

    // Get person by ID with qualifications
    Task<(Person? Person, List<Qualification> Qualifications)> GetByIdAsync(int personId);

    // Create person with qualifications (transactional)
    Task<int> CreateAsync(PersonRequest request);

    // Update person with qualifications (transactional)
    Task<bool> UpdateAsync(int personId, PersonRequest request);

    // Delete person (cascade deletes qualifications)
    Task<bool> DeleteAsync(int personId);

    // Add single qualification
    Task<int> AddQualificationAsync(int personId, QualificationRequest request);

    // Delete single qualification
    Task<bool> DeleteQualificationAsync(int qualificationId);
}