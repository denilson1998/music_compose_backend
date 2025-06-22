using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using music_compose_backend.Services;

namespace music_compose_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;

        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("suggest/harmony")]
        public async Task<IActionResult> SuggestHarmony([FromBody] AISuggestionRequest request)
        {
            try
            {
                var suggestion = await _aiService.GetHarmonySuggestion(request.MusicXml);
                return Ok(suggestion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating suggestion: {ex.Message}");
            }
        }

        [HttpPost("suggest/melody")]
        public async Task<IActionResult> SuggestMelody([FromBody] AISuggestionRequest request)
        {
            try
            {
                var suggestion = await _aiService.GetMelodySuggestion(request.MusicXml);
                return Ok(suggestion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating melody: {ex.Message}");
            }
        }

        [HttpPost("suggest/structure")]
        public async Task<IActionResult> SuggestStructure([FromBody] StyleRequest request)
        {
            try
            {
                var suggestion = await _aiService.GetStructureSuggestion(request.Style);
                return Ok(suggestion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating structure: {ex.Message}");
            }
        }

        [HttpPost("transcribe/audio")]
        public async Task<IActionResult> TranscribeFromAudio(IFormFile audio)
        {
            if (audio == null || audio.Length == 0)
                return BadRequest("No audio file uploaded.");

            try
            {
                var xmlResult = await _aiService.TranscribeAudioToMusicXml(audio);
                return Ok(new
                {
                    MusicXml = xmlResult,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Transcription failed: {ex.Message}");
            }
        }


        public class StyleRequest
        {
            public string Style { get; set; } = "";
        }

        public class AISuggestionRequest
        {
            public string MusicXml { get; set; } = "";
        }
    }
}
