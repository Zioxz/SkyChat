
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Coflnet.Sky.Chat.Models
{
    [DataContract]
    public class DbMessage
    {
        [IgnoreDataMember]
        [JsonIgnore]
        public int Id { get; set; }
        [DataMember(Name = "sender")]
        [System.ComponentModel.DataAnnotations.StringLength(32)]
        public string Sender { get; set; }
        [DataMember(Name = "level")]
        public int Level { get; set; }
        /// <summary>
        /// Content of the message
        /// </summary>
        /// <value></value>
        [DataMember(Name = "content")]
        public string Content { get; set; }
        /// <summary>
        /// What client software sent this message
        /// </summary>
        /// <value></value>
        [DataMember(Name = "clientId")]
        public int ClientId { get; set; }
        /// <summary>
        /// When this message was sent
        /// </summary>
        /// <value></value>
        [System.ComponentModel.DataAnnotations.Timestamp]
        [DataMember(Name = "timestamp")]
        public DateTime Timestamp { get; set; }
    }
}