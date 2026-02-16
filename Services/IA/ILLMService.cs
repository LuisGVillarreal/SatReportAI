namespace SatReportAI.Services.IA
{
    public interface ILLMService
    {
        Task<string> GenerateAsync(string prompt);
    }
}
