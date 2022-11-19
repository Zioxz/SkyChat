
using System.Threading.Tasks;
using Coflnet.Sky.Chat.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Confluent.Kafka;

namespace Coflnet.Sky.Chat.Services;

public class MuteService
{
    private ChatDbContext db;
    private ChatBackgroundService backgroundService;
    private IMuteProducer producer;

    public MuteService(ChatDbContext db, ChatBackgroundService backgroundService, IMuteProducer producer)
    {
        this.db = db;
        this.backgroundService = backgroundService;
        this.producer = producer;
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
        if (muteText.Contains("rule "))
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
                reason = mute.Message,
                key = tfm.WebhookAuth,
            };
            request.AddJsonBody(parameters);
            var response = await apiClient.ExecuteAsync(request);
            Console.WriteLine("mute response: " + response.Content);
        }
        await producer.Produce(mute);
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
        return Math.Max(nextLength, 1);
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

    /// <summary>
    /// Retrieves a mute or null
    /// </summary>
    /// <param name="uuid"></param>
    /// <returns></returns>
    public async Task<Mute> GetMute(string uuid)
    {
        return await db.Mute.Where(u => u.Uuid == uuid && u.Expires > DateTime.UtcNow && !u.Status.HasFlag(MuteStatus.CANCELED)).FirstOrDefaultAsync();
    }

    private async Task DisableMute(UnMute unmute, Client client, Mute mute)
    {
        mute.Status |= MuteStatus.CANCELED;
        mute.UnMuteClientId = client.Id;
        mute.UnMuter = unmute.UnMuter;
        await db.SaveChangesAsync();
    }
}

public interface IMuteProducer
{
    Task Produce(Mute mute);
}

public class MuteProducer : IMuteProducer
{
    IConfiguration config;
    private static RestClient restClient = new RestClient("https://sky.coflnet.com");
    public MuteProducer(IConfiguration config)
    {
        this.config = config;
    }

    public async Task Produce(Mute mute)
    {
        ProducerConfig producerConfig = new ProducerConfig
        {
            BootstrapServers = config["KAFKA_HOST"],
            LingerMs = 0
        };

        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
        string name = await GetName(mute);
        var message = $"🔇 User {name} was muted by {mute.Muter} for `{mute.Reason}` until <t:{new DateTimeOffset(mute.Expires).ToUnixTimeSeconds()}";
        await producer.ProduceAsync(config["TOPICS:DISCORD_MESSAGE"], new() { Value = JsonConvert.SerializeObject(new { message, channel = "mutes" }) });
    }

    private static async Task<string> GetName(Mute mute)
    {
        var name = mute.Uuid;
        var result = await restClient.ExecuteAsync(new RestRequest("/api/player/{playerUuid}/name").AddUrlSegment("playerUuid", mute.Uuid));
        try
        {
            name = JsonConvert.DeserializeObject<string>(result.Content);
        }
        catch (Exception)
        {
            Console.WriteLine("could not get name for mute " + result.Content);
        }

        return name;
    }
}