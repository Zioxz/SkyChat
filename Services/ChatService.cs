using System.Threading.Tasks;
using Coflnet.Sky.Chat.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

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
        private static RestClient restClient = new RestClient("https://sky.coflnet.com");
        private static ConcurrentQueue<DbMessage> recentMessages = new ConcurrentQueue<DbMessage>();
        static HashSet<string> BadWords = new() { " cock ", "penis ", " ass ", "b.com", "my ah", "/ah ", "/auction", "@everyone", "@here" };
        static Prometheus.Counter messagesSent = Prometheus.Metrics.CreateCounter("sky_chat_messages_sent", "Count of messages distributed");
        private ILogger<ChatService> Logger;
        private EmojiService emojiService;

        /// <summary>
        /// Creates a new instance of <see cref="ChatService"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="connection"></param>
        /// <param name="backgroundService"></param>
        /// <param name="logger"></param>
        /// <param name="emojiService"></param>
        public ChatService(ChatDbContext db, ConnectionMultiplexer connection, ChatBackgroundService backgroundService, ILogger<ChatService> logger, EmojiService emojiService)
        {
            this.db = db;
            this.connection = connection;
            this.backgroundService = backgroundService;
            Logger = logger;
            this.emojiService = emojiService;
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
            if (String.IsNullOrEmpty(message.Uuid))
                throw new ApiException("invalid_uuid", "The uuid of the sending player has to be set");
            if (message.ClientName == null)
                message.ClientName = client.Name;
            if (message.ClientName != client.Name)
                throw new ApiException("token_mismatch", "Client name does not match with the provided token");

            var existsAlready = recentMessages.Where(f => f.Sender == message.Uuid && f.Content == message.Message).Any();
            if (existsAlready)
                throw new ApiException("message_spam", "Please don't send the same message twice");
            Mute mute = await GetMute(message.Uuid);
            if (mute != default)
                throw new ApiException("user_muted", GetMuteMessage(mute));

            var dbMessage = new DbMessage()
            {
                ClientId = client.Id,
                Content = message.Message,
                Sender = message.Uuid,
                Timestamp = DateTime.Now
            };

            recentMessages.Enqueue(dbMessage);
            if (recentMessages.Count >= 10)
                recentMessages.TryDequeue(out _);

            if (BadWords.Any(word => message.Message.ToLower().Contains(word)))
                throw new ApiException("bad_words", "message contains bad words and was denied");

            if(message.Message.ToLower().Contains("get binmaster"))
                throw new ApiException("illegal_script", "Binmaster violates the hypixel terms of service. Violating the TOS can get your account banned and wiped.");

            var tries = 0;
            while (string.IsNullOrEmpty(message.Name))
            {
                var result = await restClient.ExecuteAsync(new RestRequest("/api/player/{playerUuid}/name").AddUrlSegment("playerUuid", message.Uuid));
                try
                {
                    message.Name = JsonConvert.DeserializeObject<string>(result.Content);
                }
                catch (Exception)
                {
                    Console.WriteLine(result.Content);
                    if (tries++ > 3)
                    {
                        message.Name = "invalid name";
                        break;
                    }
                }
            }

            var pubsub = connection.GetSubscriber();
            var original = message.Message;
            message.Message = emojiService.ReplaceIn(message.Message);
            var replaced = JsonConvert.SerializeObject(message);
            message.Message = original;
            await pubsub.PublishAsync("chat", replaced, CommandFlags.FireAndForget);

            db.Messages.Add(dbMessage);
            await db.SaveChangesAsync();
            _ = Task.Run(async () => await backgroundService.SendWebhooks(message));
            messagesSent.Inc();
            return true;
        }

        private static string GetMuteMessage(Mute mute)
        {
            return $"You are muted until {mute.Expires.ToString("F")} ({(DateTime.UtcNow - mute.Expires).ToString("d'd 'h'h 'm'm 's's'")}) because {mute.Message ?? "you violated a rule"}";
        }

        private async Task<Mute> GetMute(string uuid)
        {
            return await db.Mute.Where(u => u.Uuid == uuid && u.Expires > DateTime.UtcNow && !u.Status.HasFlag(MuteStatus.CANCELED)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Add a mute to an user
        /// </summary>
        /// <param name="mute"></param>
        /// <param name="clientToken"></param>
        /// <returns></returns>
        public async Task<Mute> MuteUser(Mute mute, string clientToken)
        {
            var client = backgroundService.GetClient(clientToken);
            mute.ClientId = client.Id;
            var muteText = mute.Message + mute.Reason;
            if(muteText.Contains("rule "))
            {
                // rule violation
                var mutes = await db.Mute.Where(u => u.Uuid == mute.Uuid && !u.Status.HasFlag(MuteStatus.CANCELED)).ToListAsync();
                var firstMessage = await db.Messages.Where(u => u.Sender == mute.Uuid).OrderBy(m => m.Id).FirstOrDefaultAsync();
                double nextLength = GetMuteTime(mutes, firstMessage.Timestamp);
                mute.Expires = DateTime.UtcNow + TimeSpan.FromHours(nextLength);
            }
            db.Add(mute);
            await db.SaveChangesAsync();
            if (!client.Name.Contains("tfm"))
            {
                var apiClient = new RestClient("https://chat.thom.club/");
                var request = new RestRequest("mute", Method.Post);
                var tfm = backgroundService.GetClientByName("tfm");
                if (tfm == null)
                    return mute;
                var parameters = new
                {
                    uuid = mute.Uuid,
                    muter = 267680402594988033,
                    until = (long)(mute.Expires - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds,
                    reason = GetMuteMessage(mute),
                    key = tfm.WebhookAuth,
                };
                request.AddJsonBody(parameters);
                var response = await apiClient.ExecuteAsync(request);
                Console.WriteLine("mute response: " + response.Content);
            }
            return mute;
        }

        public static double GetMuteTime(List<Mute> mutes, DateTime firstMessageTime)
        {
            var currentTime = 1;
            foreach (var item in mutes)
            {
                if ((item.Reason + item.Message).Contains("rule 1"))
                    currentTime *= 10;
                if ((item.Reason + item.Message).Contains("rule 2"))
                    currentTime *= 3;
            }
            var timeSinceJoin = firstMessageTime - DateTime.Now;
            var reduction = Math.Max(1, Math.Pow(0.7, timeSinceJoin.TotalDays / 30));
            var nextLength = currentTime / (reduction);
            return nextLength;
        }

        public async Task<UnMute> UnMuteUser(UnMute unmute, string clientToken)
        {
            var client = backgroundService.GetClient(clientToken);
            var mute = await GetMute(unmute.Uuid);
            if (mute == null)
                throw new ApiException("no_mute_found", $"There was no active mute for the user {unmute.Uuid}");

            await DisableMute(unmute, client, mute);

            if (!client.Name.Contains("tfm"))
            {
                var apiClient = new RestClient("https://chat.thom.club/");
                var request = new RestRequest("unmute", Method.Post);
                var tfm = backgroundService.GetClientByName("tfm");
                if (tfm == null)
                    return unmute;
                var parameters = new
                {
                    uuid = mute.Uuid,
                    unmuter = 267680402594988033,
                    reason = unmute.Reason,
                    key = tfm.WebhookAuth,
                };
                request.AddJsonBody(parameters);
                var response = await apiClient.ExecuteAsync(request);
                Console.WriteLine("unmute response: " + response.Content);
            }

            return unmute;
        }

        private async Task DisableMute(UnMute unmute, Client client, Mute mute)
        {
            mute.Status |= MuteStatus.CANCELED;
            mute.UnMuteClientId = client.Id;
            mute.UnMuter = unmute.UnMuter;
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a new client, generates a new secure api key before saving it into the db
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task<Client> CreateClient(Client client)
        {
            if (await db.Clients.Where(c => c.Name == client.Name).AnyAsync())
                throw new ApiException("client_exists", "A client with the same name already exists");
            var key = System.Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));
            Console.WriteLine("new key stars with " + key.Substring(0, 4));
            client.ApiKey = key;
            db.Add(client);
            await db.SaveChangesAsync();
            return client;
        }
    }
}
