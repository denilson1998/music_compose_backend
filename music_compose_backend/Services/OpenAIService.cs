using Azure;
using Melanchall.DryWetMidi.Core;
using music_compose_backend.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace music_compose_backend.Services
{
    public class OpenAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public OpenAIService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<AISuggestion> GetHarmonySuggestion(string musicXml)
        {
            var prompt = $"""
        Como asistente musical experto, analiza esta melodía en MusicXML:
        {musicXml}
        
        Sugiere una progresión armónica adecuada en formato MusicXML.
        Incluye solo el XML válido sin comentarios adicionales.
        """;

            var request = new
            {
                model = "gpt-4",
                messages = new[] { new { role = "user", content = prompt } },
                temperature = 0.7,
                max_tokens = 2000
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error calling AI service");
            }

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
            var xmlContent = result.Choices[0].Message.Content;

            // Validar y limpiar el XML devuelto
            return new AISuggestion
            {
                SuggestionType = "harmony",
                MusicXml = xmlContent,
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<AISuggestion> GetMelodySuggestion(string chordsXml)
        {
            var prompt = $"""
                        Como compositor experto, genera una melodía que se adapte a esta progresión armónica en formato MusicXML:
                        {chordsXml}

                        Devuelve solo el contenido XML válido de la melodía.
                        """;

            var request = new
            {
                model = "gpt-4",
                messages = new[] { new { role = "user", content = prompt } },
                temperature = 0.7,
                max_tokens = 2000
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error generating melody from AI service");
            }

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
            var xmlContent = result.Choices[0].Message.Content;

            return new AISuggestion
            {
                SuggestionType = "melody",
                MusicXml = xmlContent,
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<AISuggestion> GetStructureSuggestion(string style)
        {
            var prompt = $@"
                        Dado el estilo musical ""{style}"", sugiere una estructura de canción (por ejemplo: Intro, Verso, Estribillo, etc.) en formato JSON como este:
                        {{""structure"": [""Intro"", ""Verso"", ""Estribillo"", ""Verso"", ""Puente"", ""Estribillo"", ""Final""]}}
                        ";

            var request = new
            {
                model = "gpt-4",
                messages = new[] { new { role = "user", content = prompt } },
                temperature = 0.5,
                max_tokens = 500
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error generating structure from AI service");
            }

            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
            var content = result.Choices[0].Message.Content;

            return new AISuggestion
            {
                SuggestionType = "structure",
                MusicXml = content,
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<AISuggestion> TranscribeAudioToMusicXml(IFormFile audioFile)
        {


            using var stream = new MemoryStream();
            await audioFile.CopyToAsync(stream);
            stream.Position = 0;

            // Configurar API Key
            var apiKey = _config["OpenAI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "file", audioFile.FileName);
            content.Add(new StringContent("es"), "language");
            content.Add(new StringContent("whisper-1"), "model");

            // 1. Transcripción del audio con Whisper
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Whisper transcription failed: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TranscriptionResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // 2. Prompt a GPT-4 para convertir transcripción en MusicXML
            var prompt = $"""
                Eres un compositor experto. A partir de la siguiente letra de una canción, genera una melodía simple en formato MusicXML para voz principal, en clave de sol, compás 4/4, tonalidad C mayor.

                Letra:
                "{result.Text}"

                ⚠️ Devuelve únicamente el bloque XML, sin usar ```markdown ni ningún texto extra. 
                Empieza directamente con '<?xml' y termina exactamente en '</score-partwise>'.
                """;


            var request = new
            {
                model = "gpt-4",
                messages = new[] { new { role = "user", content = prompt } },
                temperature = 0.5,
                max_tokens = 2000
            };

            var chatResponse = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request);

            if (!chatResponse.IsSuccessStatusCode)
            {
                var error = await chatResponse.Content.ReadAsStringAsync();
                throw new Exception("Error generating MusicXML: " + error);
            }

            var chatResult = await chatResponse.Content.ReadFromJsonAsync<OpenAIResponse>();
            var rawXml = chatResult?.Choices[0].Message.Content ?? throw new Exception("Empty MusicXML from GPT-4");

            var cleanXml = ExtraerXmlValido(rawXml);

            return new AISuggestion
            {
                SuggestionType = "audio-transcription",
                MusicXml = cleanXml,
                Timestamp = DateTime.UtcNow
            };
        }

        private string ExtraerXmlValido(string gptResponse)
        {
            var trimmed = gptResponse.Trim();

            // Si vino con markdown (```xml)
            if (trimmed.StartsWith("```xml") || trimmed.StartsWith("```"))
            {
                var xmlStart = trimmed.IndexOf("<?xml", StringComparison.Ordinal);
                var xmlEnd = trimmed.LastIndexOf("</score-partwise>", StringComparison.Ordinal);
                if (xmlStart >= 0 && xmlEnd > xmlStart)
                {
                    return trimmed.Substring(xmlStart, xmlEnd + "</score-partwise>".Length - xmlStart).Trim();
                }
            }

            // Si ya parece válido
            if (trimmed.StartsWith("<?xml") && trimmed.Contains("</score-partwise>"))
            {
                return trimmed;
            }

            throw new Exception("Respuesta GPT no contiene XML válido");
        }


        public class OpenAIResponse
        {
            public List<Choice> Choices { get; set; } = new();

            public class Choice
            {
                public Message Message { get; set; } = new();
            }

            public class Message
            {
                public string Role { get; set; } = "";
                public string Content { get; set; } = "";
            }
        }

        public class TranscriptionResponse
        {
            public string Text { get; set; } = "";
        }
    }
}
