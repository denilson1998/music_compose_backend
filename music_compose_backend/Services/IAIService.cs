using music_compose_backend.Models;

namespace music_compose_backend.Services
{
    public interface IAIService
    {
        Task<AISuggestion> GetHarmonySuggestion(string musicXml);
        Task<AISuggestion> GetMelodySuggestion(string chordsXml);
        Task<AISuggestion> GetStructureSuggestion(string style);
        Task<AISuggestion> TranscribeAudioToMusicXml(IFormFile audioFile);

    }
}
