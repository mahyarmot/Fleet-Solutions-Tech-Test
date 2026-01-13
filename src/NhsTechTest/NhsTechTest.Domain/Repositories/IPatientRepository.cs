using NhsTechTest.Domain.Entities;

namespace NhsTechTest.Domain.Repositories;

public interface IPatientRepository
{
    /// <summary>
    /// Retrieves a patient by their unique identifier
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all patients
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<Patient>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nhsNumber"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Patient?> GetByNHSNumberAsync(string nhsNumber, CancellationToken cancellationToken);

    public Task<int> AddAsync(Patient patient, CancellationToken cancellationToken = default);

    public Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
