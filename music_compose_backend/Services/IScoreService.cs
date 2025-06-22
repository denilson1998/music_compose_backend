using music_compose_backend.Entities;

namespace music_compose_backend.Services
{
    public interface IScoreService
    {
        Task<Score> SaveScoreAsync(ScoreDto dto, string userId);
        Task<Score?> GetByIdAsync(int id);

        Task<IEnumerable<Score>> GetByUserIdAsync(string userId);
    }
}
