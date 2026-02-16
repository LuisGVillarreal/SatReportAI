using Microsoft.AspNetCore.Mvc;
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

        public AIController(
            ILLMService llmService,
            IMongoQueryService mongoService)
        {
            _llmService = llmService;
            _mongoService = mongoService;
        }

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] PromptRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Prompt))
                    return BadRequest("Prompt vacío");

                var llmResult = await _llmService.GenerateAsync(request.Prompt);

                if (string.IsNullOrWhiteSpace(llmResult))
                    return BadRequest("Respuesta vacía del modelo");

                var cleaned = llmResult
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                if (string.IsNullOrWhiteSpace(cleaned))
                    return BadRequest("El modelo no devolvió JSON válido");

                var queryDefinition = JsonConvert.DeserializeObject<MongoQueryDefinition>(cleaned);

                if (queryDefinition == null)
                    return BadRequest("Respuesta inválida del modelo");

                if (queryDefinition.Error != null)
                {
                    return BadRequest(queryDefinition.Error.ToString());
                }

                queryDefinition.RfcOwner = request.RfcOwner;

                var mongoResult = await _mongoService.ExecuteAsync(queryDefinition);

                return Ok(mongoResult);
            }
            catch (JsonReaderException)
            {
                return BadRequest("Error al interpretar el JSON generado por el modelo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }

}
