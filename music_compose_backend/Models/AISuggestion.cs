namespace music_compose_backend.Models
{
    public class AISuggestion
    {
        public string SuggestionType { get; set; }
        public string MusicXml { get; set; }

        public string MusicXmlGuitar { get; set; } = "";
        public DateTime Timestamp { get; set; } 
        public AISuggestion()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
