using demoapi.Common.Results;
using demoapi.Dtos.Scores;

namespace demoapi.Services.Scores;
    public interface IScoreService
    {
        Task<List<ScoreDto>> ListAsync(int enrollmentId, CancellationToken ct = default);

        Task<ServiceResult<ScoreDto>> UpsertAsync(ScoreRequest request, CancellationToken ct = default);

        Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default);
    }
