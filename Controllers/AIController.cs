using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using SatReportAI.Models;
using SatReportAI.Services.IA;
using SatReportAI.Services.MongoDB;

namespace SatReportAI.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AIController : ControllerBase
    {
        private readonly ILLMService _llmService;
        private readonly IMongoQueryService _mongoService;
        private readonly IMongoCollection<QuerySession> _sessionCollection;

        public AIController(
            ILLMService llmService,
            IMongoQueryService mongoService,
            IMongoDatabase database)
        {
            _llmService = llmService;
            _mongoService = mongoService;
            _sessionCollection = database.GetCollection<QuerySession>("QuerySessions");
        }

        [HttpPost("query")]
        public async Task<IActionResult> Generate([FromBody] PromptRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Prompt))
                    return BadRequest("Prompt vacío");

                var llmResult = await _llmService.GenerateAsync(request.Prompt);

                if (string.IsNullOrWhiteSpace(llmResult))
                    return BadRequest("Respuesta vacía del modelo");

                var cleaned = llmResult.Replace("```json", "").Replace("```", "").Trim();

                if (string.IsNullOrWhiteSpace(cleaned))
                    return BadRequest("El modelo no devolvió JSON válido");

                var bsonDoc = BsonDocument.Parse(cleaned);

                var queryDefinition = new MongoQueryDefinition
                {
                    Collection = bsonDoc.GetValue("collection", "").AsString,
                    Filter = bsonDoc.Contains("filter") ? bsonDoc["filter"].AsBsonDocument : null,
                    Projection = bsonDoc.Contains("projection") ? bsonDoc["projection"].AsBsonDocument : null,
                    Sort = bsonDoc.Contains("sort") ? bsonDoc["sort"].AsBsonDocument : null,
                    Error = bsonDoc.GetValue("error", "").AsString
                };

                if (!string.IsNullOrEmpty(queryDefinition.Error))
                    return BadRequest(queryDefinition.Error);

                queryDefinition.RfcOwner = request.RfcOwner;

                var sessionId = Guid.NewGuid().ToString();

                var session = new QuerySession
                {
                    Id = sessionId,
                    RfcOwner = request.RfcOwner,
                    Prompt = request.Prompt,
                    QueryDefinition = queryDefinition,
                    CreatedAt = DateTime.UtcNow
                };

                await _sessionCollection.InsertOneAsync(session);

                var (data, totalRecords) = await _mongoService.ExecuteAsync(queryDefinition, 1, request.PageSize);

                return Ok(new
                {
                    sessionId,
                    data,
                    pagination = new
                    {
                        page = 1,
                        pageSize = request.PageSize,
                        totalRecords,
                        totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPost("query/page")]
        public async Task<IActionResult> GetPage([FromBody] QueryPageRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.SessionId))
                    return BadRequest("SessionId vacío");

                var session = await _sessionCollection
                    .Find(x => x.Id == request.SessionId)
                    .FirstOrDefaultAsync();

                if (session == null)
                    return NotFound("Sesión no encontrada");

                if (session.RfcOwner != request.RfcOwner)
                    return Unauthorized("RfcOwner no coincide");

                var (data, totalRecords) = await _mongoService.ExecuteAsync(
                    session.QueryDefinition,
                    request.Page,
                    request.PageSize
                );

                return Ok(new
                {
                    sessionId = request.SessionId,
                    data,
                    pagination = new
                    {
                        page = request.Page,
                        pageSize = request.PageSize,
                        totalRecords,
                        totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }

}
