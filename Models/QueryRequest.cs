namespace SatReportAI.Models
{
    public class QueryRequest
    {
        public string Prompt { get; set; }
        public string RfcOwner { get; set; }
        public int PageSize { get; set; } = 20;
    }

    public class QueryPageRequest
    {
        public string SessionId { get; set; }
        public string RfcOwner { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class QuerySession
    {
        public string Id { get; set; }
        public string RfcOwner { get; set; }
        public string Prompt { get; set; }
        public MongoQueryDefinition QueryDefinition { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
