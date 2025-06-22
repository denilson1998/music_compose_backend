using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using music_compose_backend.Entities;
using music_compose_backend.Services;
using System.Security.Claims;

namespace music_compose_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoresController : ControllerBase
    {
        private readonly IScoreService _scoreService;
        private readonly IMusicXmlProcessor _musicXmlProcessor;

        public ScoresController(IScoreService scoreService, IMusicXmlProcessor musicXmlProcessor)
        {
            _scoreService = scoreService;
            _musicXmlProcessor = musicXmlProcessor;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate([FromBody] ScoreDto scoreDto)
        {
            
            try
            {
                // Validar y procesar MusicXML
                var validationResult = _musicXmlProcessor.Validate(scoreDto.MusicXml);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }

                var score = await _scoreService.SaveScoreAsync(scoreDto, scoreDto.userId);
                return Ok(score);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing score: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var score = await _scoreService.GetByIdAsync(id);
            if (score == null) return NotFound();

            // Verificar permisos de acceso
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (score.UserId != userId && !score.IsPublic)
            {
                return Forbid();
            }

            return Ok(score);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            try
            {
                var scores = await _scoreService.GetByUserIdAsync(userId);
                return Ok(scores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving scores: {ex.Message}");
            }
        }

    }
}
