using System.Text.Json.Serialization;

namespace CalendarAssistant.Models
{
    public class ClassifierResponse
    {
        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }
        [JsonPropertyName("ismeetinginvite")]
        public bool IsMeetingInvite { get; set; }
    }
}
