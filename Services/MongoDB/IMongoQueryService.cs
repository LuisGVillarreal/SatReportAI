using MongoDB.Bson;
using SatReportAI.Models;

namespace SatReportAI.Services.MongoDB
{
    public interface IMongoQueryService
    {
        Task<(List<object> Data, long TotalRecords)> ExecuteAsync(MongoQueryDefinition query, int page, int pageSize);
    }
}
