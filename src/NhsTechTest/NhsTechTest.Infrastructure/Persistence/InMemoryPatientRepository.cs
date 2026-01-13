using NhsTechTest.Domain.Entities;
using NhsTechTest.Domain.Repositories;

namespace NhsTechTest.Infrastructure.Persistence;

public class InMemoryPatientRepository : IPatientRepository
{
    private readonly List<Patient> _patients;
    private int _nextId;

    public InMemoryPatientRepository()
    {
        _patients =
        [
            Patient.Create(
                id: 1,
                nhsNumber: "485 777 3456",
                name: "Sarah Johnson",
                dateOfBirth: new DateTime(1985, 3, 15),
                gpPractice: "North Medical Centre"),

            Patient.Create(
                id: 2,
                nhsNumber: "943 476 5892",
                name: "James Anderson",
                dateOfBirth: new DateTime(1978, 7, 22),
                gpPractice: "Wst Health Clinic"),

            Patient.Create(
                id: 3,
                nhsNumber: "562 839 1047",
                name: "Emily Williams",
                dateOfBirth: new DateTime(1992, 11, 8),
                gpPractice: "Greenfield Surgery"),

            Patient.Create(
                id: 4,
                nhsNumber: "721 304 8569",
                name: "Mohammed Ali",
                dateOfBirth: new DateTime(1965, 5, 30),
                gpPractice: "Central Medical Practice"),

            Patient.Create(
                id: 5,
                nhsNumber: "894 652 7103",
                name: "Catherine Brown",
                dateOfBirth: new DateTime(2001, 9, 17),
                gpPractice: "Riverside Medical Centre")
        ];
    }

    public Task<IEnumerable<Patient>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<IEnumerable<Patient>>(_patients);
    }

    public Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var patient = _patients.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(patient);
    }

    public Task<Patient?> GetByNHSNumberAsync(string nhsNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var patient = _patients.FirstOrDefault(p => p.NHSNumber == nhsNumber);
        return Task.FromResult(patient);
    }

    public Task<int> AddAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Assign new ID
        var patientWithId = Patient.Create(
            id: _nextId,
            nhsNumber: patient.NHSNumber,
            name: patient.Name,
            dateOfBirth: patient.DateOfBirth,
            gpPractice: patient.GPPractice);

        _patients.Add(patientWithId);
        var assignedId = _nextId;
        _nextId++;

        return Task.FromResult(assignedId);
    }

    public Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingPatient = _patients.FirstOrDefault(p => p.Id == patient.Id);
        if (existingPatient is not null)
        {
            _patients.Remove(existingPatient);
            _patients.Add(patient);
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var patient = _patients.FirstOrDefault(p => p.Id == id);
        if (patient is not null)
        {
            _patients.Remove(patient);
        }

        return Task.CompletedTask;
    }
}
