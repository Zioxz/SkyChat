
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Coflnet.Sky.Chat.Models
{
    [DataContract]
    public class Report
    {
        [IgnoreDataMember]
        [JsonIgnore]
        public int Id { get; set; }
        [DataMember(Name = "sender")]
        [System.ComponentModel.DataAnnotations.StringLength(32)]
        public string Sender { get; set; }
        [DataMember(Name = "message")]
        public DbMessage Message { get; set; }
        [DataMember(Name = "reason")]
        public string Reason { get; set; }
        [System.ComponentModel.DataAnnotations.Timestamp]
        [DataMember(Name = "timestamp")]
        public DateTime Timestamp { get; set; }
    }
}