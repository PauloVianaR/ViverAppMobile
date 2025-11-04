using Microsoft.AspNetCore.Mvc;

namespace ViverAppVideoHub.Controllers
{
    [ApiController]
    [Route("room")]
    public class RoomController : ControllerBase
    {
        [HttpGet("{roomId}/count")]
        public IActionResult GetRoomCount(string roomId)
        {
            var count = VideoHub.GetRoomCount(roomId);
            return Ok(new { count });
        }
    }
}
