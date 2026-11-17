using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebApplication6.Service;

namespace WebApplication6.Controllers
{
    [ApiController]
    [Route("RecipientStaff")]
    public class RecipientStaffController : ControllerBase
    {
        private readonly ServiceRecipientStaff _service;

        public RecipientStaffController(ServiceRecipientStaff service)
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

        [HttpGet("categories")]
        public async Task<IActionResult> GetFoodCategories(int userId)
        {
            var categories = await _service.GetFoodCategories(userId);
            return Ok(categories);
        }

        [HttpGet("food")]
        public async Task<IActionResult> GetAvailableFood(int userId)
        {
            var foods = await _service.GetAvailableFood(userId);
            return Ok(foods);
        }

        [HttpGet("food/{foodId:int}")]
        public async Task<IActionResult> GetFoodById(int userId, int foodId)
        {
            var food = await _service.GetFoodById(userId, foodId);
            return Ok(food);
        }

        [HttpGet("food/{foodId:int}/images")]
        public async Task<IActionResult> GetFoodImages(int userId, int foodId)
        {
            var images = await _service.GetFoodImages(userId, foodId);
            return Ok(images);
        }

        [HttpPost("requests")]
        public async Task<IActionResult> AddPickupRequest(int userId, [FromBody] JObject data)
        {
            var request = await _service.AddPickupRequest(userId, data);
            return Ok(request);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetPickupRequests(int userId)
        {
            var requests = await _service.GetPickupRequests(userId);
            return Ok(requests);
        }

        [HttpGet("requests/{requestId:int}")]
        public async Task<IActionResult> GetPickupRequestById(int userId, int requestId)
        {
            var request = await _service.GetPickupRequestById(userId, requestId);
            return Ok(request);
        }

        [HttpPut("requests/{requestId:int}/cancel")]
        public async Task<IActionResult> CancelPickupRequest(int userId, int requestId)
        {
            var request = await _service.CancelPickupRequest(userId, requestId);
            return Ok(request);
        }

        [HttpGet("pickups")]
        public async Task<IActionResult> GetPickupHistory(int userId)
        {
            var pickups = await _service.GetPickupHistory(userId);
            return Ok(pickups);
        }
    }
}
