using System.Threading;
using System.Threading.Tasks;
using Coflnet.Sky.Chat.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Extensions.Logging;
using Coflnet.Sky.Chat.Controllers;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace Coflnet.Sky.Chat.Services
{

    public class ChatBackgroundService : BackgroundService
    {
        private IServiceScopeFactory scopeFactory;
        private IConfiguration config;
        private ILogger<ChatBackgroundService> logger;
        private Prometheus.Counter consumeCount = Prometheus.Metrics.CreateCounter("sky_chat_conume", "How many messages were consumed");

        private ConcurrentDictionary<string, Client> Clients = new();
        private List<(string WebHook, string WebhookAuth)> Webhooks = new();

        public bool Ready => Clients.Count > 0;

        public ChatBackgroundService(
            IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<ChatBackgroundService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.config = config;
            this.logger = logger;
        }

        internal Client GetClient(string clientToken)
        {
            if (!Clients.TryGetValue(clientToken, out Client client))
                throw new ApiException("invalid_token", "Invalid client Id/unkown client");
            return client;
        }

        /// <summary>
        /// Called by asp.net on startup
        /// </summary>
        /// <param name="stoppingToken">is canceled when the applications stops</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
            // make sure all migrations are applied
            // runs syncronously to avoid multiple migrations starting simotaniously 
            context.Database.Migrate();

            while (!stoppingToken.IsCancellationRequested)
            {
                Clients = new ConcurrentDictionary<string, Client>(await context.Clients.ToDictionaryAsync(c => c.ApiKey));
                //Webhooks = Clients.Select(c => (c.Value.WebHook, c.Value.WebhookAuth, c)).Where(w => !string.IsNullOrEmpty(w.WebHook)).ToList();
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

            }

        }

        internal async Task SendWebhooks(ChatMessage message)
        {
            var client = new HttpClient();
            var serialized = JsonConvert.SerializeObject(message);
            await Task.WhenAll(Clients.Select(c =>
            {
                var hook = c.Value;
                var content = new StringContent(serialized, Encoding.UTF8, "application/json");

                if (c.Value.Name.Contains("tfm"))
                    content = new StringContent(JsonConvert.SerializeObject(new {
                        uuid = message.Uuid,
                        isPremium = true,
                        message = message.Message,
                        apiKey = config["TFM_KEY"]
                    }), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, hook.WebHook)
                {
                    Content = content
                };
                request.Headers.Add("Authorization", hook.WebhookAuth);
                return client.SendAsync(request);
            }));
        }

        private ChatService GetService()
        {
            return scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ChatService>();
        }
    }
}