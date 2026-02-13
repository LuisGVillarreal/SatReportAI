using SatReportAI.Services;
using System.Text;
using System.Text.Json;

public class GeminiService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        var section = _configuration.GetSection("AppSettings:LLM:Gemini");
        var apiKey = section["ApiKey"];
        var model = section["Model"];
        var baseUrl = section["BaseUrl"];

        var url = $"{baseUrl}/models/{model}:generateContent";

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

        //Leer plantilla desde archivo
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Prompts", "mongo_cfdi.txt");
        var template = await File.ReadAllTextAsync(templatePath);
        var finalPrompt = template.Replace("{{prompt}}", prompt);

        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = finalPrompt
                        }
                    }
                }
            }
        };


        var json = JsonSerializer.Serialize(body);

        var response = await _httpClient.PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(result);

        var text = doc
            .RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? string.Empty;
    }
}
