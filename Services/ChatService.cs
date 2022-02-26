using System.Threading.Tasks;
using Coflnet.Sky.Chat.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Concurrent;

namespace Coflnet.Sky.Chat.Services
{
    /// <summary>
    /// Core service handling validation and distribution of messages
    /// </summary>
    public class ChatService
    {
        private ChatDbContext db;
        private ConnectionMultiplexer connection;
        private ChatBackgroundService backgroundService;
        private RestClient restClient = new RestClient("https://sky.coflnet.com");
        private ConcurrentQueue<DbMessage> recentMessages = new ConcurrentQueue<DbMessage>();
        Prometheus.Counter messagesSent = Prometheus.Metrics.CreateCounter("sky_chat_messages_sent", "Count of messages distributed");

        /// <summary>
        /// Creates a new instance of <see cref="ChatService"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="connection"></param>
        /// <param name="backgroundService"></param>
        public ChatService(ChatDbContext db, ConnectionMultiplexer connection, ChatBackgroundService backgroundService)
        {
            this.db = db;
            this.connection = connection;
            this.backgroundService = backgroundService;
        }

        /// <summary>
        /// Send a new message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="clientToken"></param>
        /// <returns></returns>
        public async Task<bool> SendMessage(ChatMessage message, string clientToken)
        {
            var client = backgroundService.GetClient(clientToken);
            if(String.IsNullOrEmpty(message.Uuid))
                throw new ApiException("invalid_uuid","The uuid of the sending player has to be set");
            if(message.ClientName == null)
                message.ClientName = client.Name;
            if(message.ClientName != client.Name)
                throw new ApiException("token_mismatch", "Client name does not match with the provided token");
            /*var mute = await db.Mute.Where(u=>u.Uuid == message.Uuid && u.Expires > DateTime.Now).FirstOrDefaultAsync();
            if(mute != default)
                throw new ApiException("user_muted", $"You are muted until {mute.Expires} because {mute.Message}");*/
            var existsAlready = recentMessages.Where(f => f.Sender == message.Uuid && f.Content == message.Message).Any();
            if(existsAlready)
                throw new ApiException("message_spam", "Please don't send the same message twice");
            var dbMessage = new DbMessage()
            {
                ClientId = client.Id,
                Content = message.Message,
                Sender = message.Uuid,
                Timestamp = DateTime.Now
            };
            
            db.Messages.Add(dbMessage);
            recentMessages.Enqueue(dbMessage);
            if(recentMessages.Count >= 10)
                recentMessages.TryDequeue(out _);
            var dbSave = db.SaveChangesAsync();
            if(string.IsNullOrEmpty(message.Name))
            {
                var result = await restClient.ExecuteAsync(new RestRequest("/api/player/{playerUuid}/name").AddUrlSegment("playerUuid", message.Name));
                message.Name = result.Content;
            }
            var pubsub = connection.GetSubscriber();
            await pubsub.PublishAsync("chat", JsonConvert.SerializeObject(message), CommandFlags.FireAndForget);
            _ = Task.Run(async()=>await backgroundService.SendWebhooks(message));
            await dbSave;
            messagesSent.Inc();
            return true;
        }

        /// <summary>
        /// Add a mute to an user
        /// </summary>
        /// <param name="mute"></param>
        /// <returns></returns>
        public async Task<Mute> MuteUser(Mute mute)
        {
            db.Add(mute);
            await db.SaveChangesAsync();
            return mute;
        }

        /// <summary>
        /// Creates a new client, generates a new secure api key before saving it into the db
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task<Client> CreateClient(Client client)
        {
            if(await db.Clients.Where(c=>c.Name == client.Name).AnyAsync())
                throw new ApiException("client_exists", "A client with the same name already exists");
            var key = System.Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));
            Console.WriteLine("new key stars with " + key.Substring(0,4));
            client.ApiKey = key;
            db.Add(client);
            await db.SaveChangesAsync();
            return client;
        }
    }
}
