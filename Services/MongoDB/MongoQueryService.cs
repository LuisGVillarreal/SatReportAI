using MongoDB.Bson;
using MongoDB.Driver;
using SatReportAI.Models;
using System.Text.Json;

namespace SatReportAI.Services.MongoDB
{
    public class MongoQueryService : IMongoQueryService
    {
        private readonly IMongoDatabase _database;

        public MongoQueryService(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<string> ExecuteAsync(MongoQueryDefinition query)
        {
            if (string.IsNullOrWhiteSpace(query.Collection))
                throw new Exception("Collection inválida");

            if (string.IsNullOrWhiteSpace(query.RfcOwner))
                throw new Exception("RfcOwner inválido");

            var collection = _database.GetCollection<BsonDocument>(query.Collection);

            var builder = Builders<BsonDocument>.Filter;
            var filters = new List<FilterDefinition<BsonDocument>>
            {
                builder.Eq("RfcOwner", query.RfcOwner) // Filtro obligatorio primero
            };

            if (query.Filter != null)
            {
                var filterDoc = BsonDocument.Parse(
                    JsonSerializer.Serialize(query.Filter)
                );

                filters.Add(new BsonDocumentFilterDefinition<BsonDocument>(filterDoc));
            }

            var finalFilter = builder.And(filters);

            var find = collection.Find(finalFilter);

            if (query.Projection != null)
            {
                var projectionDoc = BsonDocument.Parse(
                    JsonSerializer.Serialize(query.Projection)
                );

                find = find.Project<BsonDocument>(projectionDoc);
            }

            if (query.Sort != null)
            {
                var sortDoc = BsonDocument.Parse(
                    JsonSerializer.Serialize(query.Sort)
                );

                find = find.Sort(sortDoc);
            }

            var result = await find.ToListAsync();

            return result.ToJson();
        }
    }
}
