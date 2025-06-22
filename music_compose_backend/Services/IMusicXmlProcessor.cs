using System.ComponentModel.DataAnnotations;

namespace music_compose_backend.Services
{
    public interface IMusicXmlProcessor
    {
        ValidationResult Validate(string musicXml);
    }
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();
    }
}
