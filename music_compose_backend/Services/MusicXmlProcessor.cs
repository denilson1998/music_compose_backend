using System.Xml;

namespace music_compose_backend.Services
{
    public class MusicXmlProcessor : IMusicXmlProcessor
    {
        public ValidationResult Validate(string musicXml)
        {
            var result = new ValidationResult();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(musicXml);
            }
            catch (XmlException ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Invalid XML: {ex.Message}");
            }

            return result;
        }
    }
}
