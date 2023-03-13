using System;
using NUnit.Framework;

namespace Coflnet.Sky.Chat.Services;

/// <summary>
/// Tests for <see cref="ChatService"/>
/// </summary>
public class ChatServiceTests
{
    private Models.Mute rule1 = new() { Message = "rule 1" };
    private Models.Mute rule2 = new() { Message = "rule 2" };
    /// <summary>
    /// New user should receive about one hour
    /// </summary>
    [Test]
    public void MuteTimeNewuser()
    {
        var time = MuteService.GetMuteTime(new(), DateTime.UtcNow);
        Assert.That(time, Is.EqualTo(1).Within(0.00001));
    }

    /// <summary>
    /// Two times rule 1 = 100 hours
    /// </summary>
    [Test]
    public void MuteTimeNewTwoTimesRule1()
    {
        var time = MuteService.GetMuteTime(new() { rule1, rule1 }, DateTime.UtcNow);
        Assert.That(time, Is.EqualTo(100).Within(0.00001));
    }
    [Test]
    public void MuteTimeNewThreeTimesRule2()
    {
        var time = MuteService.GetMuteTime(new() { rule2,rule2,rule2 }, DateTime.UtcNow);
        Assert.That(time, Is.EqualTo(27).Within(0.00001));
    }
    [Test]
    public void MuteTimeReducesthirdPerMonth()
    {
        var time = MuteService.GetMuteTime(new() { rule1 }, DateTime.UtcNow - TimeSpan.FromDays(30));
        Assert.That(time, Is.EqualTo(7).Within(0.00001));
    }
    [Test]
    public void MuteTimeReducesthirdPerMonthLong()
    {
        var time = MuteService.GetMuteTime(new() { rule1 }, DateTime.UtcNow - TimeSpan.FromDays(60));
        Assert.That(time, Is.EqualTo(4.9).Within(0.00001));
    }
}

