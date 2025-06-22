using System.ComponentModel.DataAnnotations;

namespace music_compose_backend.Entities
{
    public class Score
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Composer { get; set; } = "";
        public string Instrumentation { get; set; } = "";
        public string Clef { get; set; } = "";
        public string TimeSignature { get; set; } = "";
        public string KeySignature { get; set; } = "";
        public string MusicXml { get; set; } = "";
        public bool IsPublic { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public class ScoreDto
    {
        public int? Id { get; set; }
        public string Title { get; set; } = "";
        public string Composer { get; set; } = "";
        public string Instrumentation { get; set; } = "";
        public string Clef { get; set; } = "";
        public string TimeSignature { get; set; } = "";
        public string KeySignature { get; set; } = "";
        public string MusicXml { get; set; } = "";
        public bool IsPublic { get; set; } = false;

        public string userId { get; set; }
    }
}
