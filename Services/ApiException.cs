namespace Coflnet.Sky.Chat.Services
{
    public class ApiException : hypixel.CoflnetException
    {
        public ApiException(string slug, string message) : base(slug, message)
        {
        }
    }
}
