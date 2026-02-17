using MongoDB.Bson;

namespace SatReportAI.Models
{
    public class MongoQueryDefinition
    {
        public string RfcOwner { get; set; } = string.Empty;
        public string Collection { get; set; } = string.Empty;
        public BsonDocument? Filter { get; set; }
        public BsonDocument? Projection { get; set; }
        public BsonDocument? Sort { get; set; }
        public string? Error { get; set; }
    }
}
