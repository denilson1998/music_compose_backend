using Microsoft.EntityFrameworkCore;
using music_compose_backend.Database;
using music_compose_backend.Entities;
using System;

namespace music_compose_backend.Services
{
    public class ScoreService : IScoreService
    {
        private readonly ApplicationDbContext _context;

        public ScoreService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Score> SaveScoreAsync(ScoreDto dto, string userId)
        {
            Score score;

            if (dto.Id.HasValue)
            {
                score = await _context.Scores.FindAsync(dto.Id.Value)
                        ?? throw new Exception("Score not found");

                if (score.UserId != userId)
                    throw new UnauthorizedAccessException();

                score.Title = dto.Title;
                score.MusicXml = dto.MusicXml;
                score.IsPublic = dto.IsPublic;
                score.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                score = new Score
                {
                    Title = dto.Title,
                    MusicXml = dto.MusicXml,
                    IsPublic = dto.IsPublic,
                    UserId = userId,
                    Composer = dto.Composer,
                    Instrumentation = dto.Instrumentation,
                    Clef = dto.Clef,
                    TimeSignature = dto.TimeSignature,
                    KeySignature = dto.KeySignature,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Scores.Add(score);
            }

            await _context.SaveChangesAsync();
            return score;
        }

        public async Task<Score?> GetByIdAsync(int id)
        {
            return await _context.Scores.FindAsync(id);
        }

        public async Task<IEnumerable<Score>> GetByUserIdAsync(string userId)
        {
            return await _context.Scores
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}
