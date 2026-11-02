namespace Rafiq.Services
{
    public interface IDeepSeekService
    {
        Task<string> AskAI ( string message );
    }
}