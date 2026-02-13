namespace SatReportAI.Services
{
    public interface ILLMService
    {
        Task<string> GenerateAsync(string prompt);
    }
}
