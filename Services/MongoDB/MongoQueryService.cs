using MongoDB.Bson;
using MongoDB.Driver;
using SatReportAI.Models;

namespace SatReportAI.Services.MongoDB
{
    public class MongoQueryService : IMongoQueryService
    {
        private readonly IMongoDatabase _database;

        public MongoQueryService(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<(List<object> Data, long TotalRecords)> ExecuteAsync(
            MongoQueryDefinition query,
            int page,
            int pageSize)
        {
            if (string.IsNullOrWhiteSpace(query.Collection))
                throw new Exception("Collection inválida");

            if (string.IsNullOrWhiteSpace(query.RfcOwner))
                throw new Exception("RfcOwner inválido");

            var collection = _database.GetCollection<BsonDocument>(query.Collection);

            var builder = Builders<BsonDocument>.Filter;

            var filters = new List<FilterDefinition<BsonDocument>>
            {
                builder.Eq("RfcOwner", query.RfcOwner)
            };

            if (query.Filter != null)
            {
                filters.Add(new BsonDocumentFilterDefinition<BsonDocument>(query.Filter));
            }

            var finalFilter = builder.And(filters);

            var find = collection.Find(finalFilter);

            if (query.Projection != null)
            {
                find = find.Project<BsonDocument>(query.Projection);
            }

            if (query.Sort != null)
            {
                find = find.Sort(query.Sort);
            }

            var totalRecords = await collection.CountDocumentsAsync(finalFilter);

            var list = await find
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var result = list
            .Select(x => BsonTypeMapper.MapToDotNetValue(x))
            .ToList();

            return (result, totalRecords);
        }
    }
}
