using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkOtus.Shared.Event.Kafka;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace SocialNetworkOtus.Applications.Backend.MainApi.Controllers
{
    [Route("api/post/feed/posted")]
    [ApiController]
    public class PostWebSocketController : ControllerBase
    {
        private readonly PostConsumer _postConsumer;

        public PostWebSocketController(PostConsumer postConsumer)
        {
            _postConsumer = postConsumer;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var context = ControllerContext.HttpContext;
            if (!context.WebSockets.IsWebSocketRequest)
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            
            var user = context.Request.Query["user"];
            if (string.IsNullOrEmpty(user))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            _postConsumer.CreateConnection(user, webSocket);

            while (webSocket.State != WebSocketState.Closed && webSocket.State != WebSocketState.Aborted)
            {
                
            }

            _postConsumer.CloseConnection(user);

            return new EmptyResult();
        }
    }
}
