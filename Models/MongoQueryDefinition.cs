using Newtonsoft.Json;

namespace SatReportAI.Models
{
    public class MongoQueryDefinition
    {
        [JsonProperty("rfcowner")]
        public string RfcOwner { get; set; } = string.Empty;

        [JsonProperty("collection")]
        public string Collection { get; set; } = string.Empty;

        [JsonProperty("filter")]
        public object Filter { get; set; } = new();

        [JsonProperty("projection")]
        public object? Projection { get; set; }

        [JsonProperty("sort")]
        public object? Sort { get; set; }
        [JsonProperty("error")]
        public object? Error { get; set; }
    }
}
