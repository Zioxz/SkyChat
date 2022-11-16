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
        private readonly MuteService muteService;

        /// <summary>
        /// Creates a new instance of <see cref="ChatController"/>
        /// </summary>
        /// <param name="service"></param>
        public ChatController(ChatService service, MuteService muteService)
        {
            this.service = service;
            this.muteService = muteService;
        }

        /// <summary>
        /// Tracks a flip
        /// </summary>
        /// <param name="flip"></param>
        /// <param name="authorization"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("send")]
        public async Task<ChatMessage> SendMessage([FromBody] ChatMessage flip, [FromHeader]string authorization)
        {
            AssertAuthHeader(authorization);
            await service.SendMessage(flip, authorization);
            return flip;
        }

        private static void AssertAuthHeader(string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
                throw new ApiException("missing_authorization", "The required authorization header wasn't passed. Set it to the token you api received.");
        }

        /// <summary>
        /// Create a nw Client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("internal/client")]
        public async Task<CientCreationResponse> CreateClient([FromBody] Client client)
        {
            return new CientCreationResponse(await service.CreateClient(client));
        }

        /// <summary>
        /// Create a new mute for an user
        /// </summary>
        /// <param name="mute">Data about the mute</param>
        /// <param name="authorization"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("mute")]
        public async Task<Mute> MuteUser([FromBody] Mute mute, [FromHeader]string authorization)
        {
            AssertAuthHeader(authorization);
            return await muteService.MuteUser(mute, authorization);
        }
        /// <summary>
        /// Create a new mute for an user
        /// </summary>
        /// <param name="mute">Data about the mute</param>
        /// <param name="authorization"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("mute")]
        public async Task<UnMute> UnMuteUser([FromBody] UnMute mute, [FromHeader]string authorization)
        {
            AssertAuthHeader(authorization);
            return await muteService.UnMuteUser(mute, authorization);
        }
    }
}
