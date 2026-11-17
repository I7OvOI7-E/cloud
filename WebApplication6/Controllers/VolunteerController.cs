using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebApplication6.Service;

namespace WebApplication6.Controllers
{
    [ApiController]
    [Route("Volunteer")]
    public class VolunteerController : ControllerBase
    {
        private readonly ServiceVolunteer _service;

        public VolunteerController(ServiceVolunteer service)
        {
            _service = service;
        }

        [HttpPost("createuser")]
        public async Task<IActionResult> CreateUser([FromBody] JObject data)
        {
            var user = await _service.CreateUser(data);
            return Ok(user);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _service.GetUsers();
            return Ok(users);
        }

        [HttpGet("users/{userId:int}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _service.GetUserById(userId);
            return Ok(user);
        }

        [HttpPut("users/{userId:int}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] JObject data)
        {
            var user = await _service.UpdateUser(userId, data);
            return Ok(user);
        }

        [HttpDelete("users/{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            await _service.DeleteUser(userId);
            return Ok(new { Message = "User deleted." });
        }

        [HttpGet("collections")]
        public async Task<IActionResult> GetCollections(int userId)
        {
            var collections = await _service.GetCollections(userId);
            return Ok(collections);
        }

        [HttpGet("collections/{requestId:int}")]
        public async Task<IActionResult> GetCollectionById(int userId, int requestId)
        {
            var collection = await _service.GetCollectionById(userId, requestId);
            return Ok(collection);
        }

        [HttpGet("pickups")]
        public async Task<IActionResult> GetPickupHistory(int userId)
        {
            var pickups = await _service.GetPickupHistory(userId);
            return Ok(pickups);
        }
    }
}
