
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

    [DataContract]
    public class ChatMessage
    {
        /// <summary>
        /// The uuid of the sender
        /// </summary>
        /// <value></value>
        [DataMember(Name = "uuid")]
        [System.ComponentModel.DataAnnotations.StringLength(32)]
        public string Uuid { get; set; }
        /// <summary>
        /// If available the name of the sender
        /// </summary>
        /// <value></value>
        [DataMember(Name = "name")]
        [System.ComponentModel.DataAnnotations.MaxLength(16)]
        public string Name { get; set; }
        /// <summary>
        /// What color/prefix the sender has, if empty the color will be white and message should be gray
        /// </summary>
        /// <value></value>
        [DataMember(Name = "prefix")]
        public string Prefix { get; set; }
        /// <summary>
        /// Content of the message
        /// </summary>
        /// <value></value>
        [DataMember(Name = "message")]
        public string Message { get; set; }
        /// <summary>
        /// What client software sent this message
        /// </summary>
        /// <value></value>
        [DataMember(Name = "clientName")]
        public string ClientName { get; set; }
    }
}