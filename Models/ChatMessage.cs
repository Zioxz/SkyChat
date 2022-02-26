using System.Runtime.Serialization;

namespace Coflnet.Sky.Chat.Models
{
    /*
    {
        "uuid":"mcUuid",
        "name":"(auto filled if not present)",
        "prefix":"color/rank prefix",
        "message":"conent of message",
        "clientName":"(autofilled based on auth header)"
    }
    */
    /// <summary>
    /// 
    /// </summary>
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