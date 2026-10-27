using demoapi.Dtos.Students;
using demoapi.Models;
using demoapi.Data;
using demoapi.Dtos;
using demoapi.Services;
using demoapi.Common.Results;
namespace demoapi.Services.Students;

public interface IStudentService
{
    Task<PagedResponse<StudentDto>> ListAsync(
        string? name, string? email, string sortBy, string sortOrder,
        int pageNumber, int pageSize, CancellationToken ct = default);

    Task<ServiceResult<StudentDto>> GetAsync(int id, CancellationToken ct = default);

    Task<ServiceResult<StudentDto>> CreateAsync(CreateStudentRequest request, CancellationToken ct = default);

    Task<ServiceResult<StudentDto>> UpdateAsync(int id, UpdateStudentRequest request, CancellationToken ct = default);

    Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default);
}