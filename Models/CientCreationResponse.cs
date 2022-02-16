namespace Coflnet.Sky.Chat.Models
{
    /// <summary>
    /// Contains the client and its api key (only place where the api key is visible)
    /// </summary>
    public class CientCreationResponse
    {
        public Client Client { get; set; }
        public string ApiKey { get; set; }

        public CientCreationResponse(Client client)
        {
            Client = client;
            ApiKey = client.ApiKey;
        }
    }
}