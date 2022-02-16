
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Coflnet.Sky.Chat.Models
{
    [DataContract]
    public class Mute
    {
        /// <summary>
        /// Internal id for updating
        /// </summary>
        /// <value></value>
        [IgnoreDataMember]
        [JsonIgnore]
        public int Id { get; set; }
        /// <summary>
        /// Uuid of the target user
        /// </summary>
        /// <value></value>
        [DataMember(Name = "target")]
        [System.ComponentModel.DataAnnotations.StringLength(32)]
        public string Uuid { get; set; }
        /// <summary>
        /// Message for the user
        /// </summary>
        /// <value></value>
        [DataMember(Name = "message")]
        public string Message { get; set; }
        /// <summary>
        /// Internal reason
        /// </summary>
        /// <value></value>
        [DataMember(Name = "reason")]
        public string Reason { get; set; }
        /// <summary>
        /// When this was created
        /// </summary>
        /// <value></value>
        [System.ComponentModel.DataAnnotations.Timestamp]
        [DataMember(Name = "timestamp")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Until when this is active
        /// </summary>
        /// <value></value>
        [DataMember(Name = "expires")]
        public DateTime Expires { get; set; }
    }
}