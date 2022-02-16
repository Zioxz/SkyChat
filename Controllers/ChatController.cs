using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Coflnet.Sky.Chat.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using Coflnet.Sky.Chat.Services;

namespace Coflnet.Sky.Chat.Controllers
{
    /// <summary>
    /// Main Controller handling tracking
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService service;

        /// <summary>
        /// Creates a new instance of <see cref="ChatController"/>
        /// </summary>
        /// <param name="service"></param>
        public ChatController(ChatService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Tracks a flip
        /// </summary>
        /// <param name="flip"></param>
        /// <param name="authorization"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("send")]
        public async Task<ChatMessage> TrackFlip([FromBody] ChatMessage flip, [FromHeader]string authorization)
        {
            if(string.IsNullOrEmpty(authorization))
                throw new ApiException("missing_authorization", "The required authorization header wasn't passed. Set it to the token you api received.");
            await service.SendMessage(flip, authorization);
            return flip;
        }


        /// <summary>
        /// Create a nw Client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("client")]
        public async Task<CientCreationResponse> TrackFlip([FromBody] Client client)
        {
            return new CientCreationResponse(await service.CreateClient(client));
        }

        /// <summary>
        /// Create a nw Client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("mute")]
        public async Task<Mute> MuteUser([FromBody] Mute mute)
        {
            return await service.MuteUser(mute);
        }
    }
}
