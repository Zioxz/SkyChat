namespace Coflnet.Sky.Chat.Services
{
    public class ApiException : Coflnet.Sky.Core.CoflnetException
    {
        public ApiException(string slug, string message) : base(slug, message)
        {
        }
    }
}
