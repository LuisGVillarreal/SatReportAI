using Microsoft.AspNetCore.Mvc;
using SatReportAI.Models;
using SatReportAI.Services;

namespace SatReportAI.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AIController : ControllerBase
    {
        private readonly ILLMService _llmService;

        public AIController(ILLMService llmService)
        {
            _llmService = llmService;
        }

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] PromptRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest("Prompt vacío");

            var result = await _llmService.GenerateAsync(request.Prompt);

            return Ok(result);
        }
    }
}
