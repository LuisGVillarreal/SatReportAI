using SatReportAI.Models;

namespace SatReportAI.Services.MongoDB
{
    public interface IMongoQueryService
    {
        Task<string> ExecuteAsync(MongoQueryDefinition query);
    }
}
