using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebApplication6.Service;

namespace WebApplication6.Controllers
{
    [ApiController]
    [Route("SchoolStaff")]
    public class SchoolStaffController : ControllerBase
    {
        private readonly ServiceSchoolStaff _service;

        public SchoolStaffController(ServiceSchoolStaff service)
        {
            _service = service;
        }

        [HttpGet("organizations")]
        public async Task<IActionResult> GetOrganizations()
        {
            var orgs = await _service.GetOrganizations();
            return Ok(orgs);
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
        public async Task<IActionResult> GetAllFoodListings(int userId)
        {
            var foods = await _service.GetAllFoodListings(userId);
            return Ok(foods);
        }

        [HttpGet("food/{foodId:int}")]
        public async Task<IActionResult> GetFoodListingById(int userId, int foodId)
        {
            var food = await _service.GetFoodListingById(userId, foodId);
            return Ok(food);
        }

        [HttpPost("food")]
        public async Task<IActionResult> AddFoodListing(int userId, [FromBody] JObject data)
        {
            var food = await _service.AddFoodListing(userId, data);
            return Ok(food);
        }

        [HttpPut("food/{foodId:int}")]
        public async Task<IActionResult> UpdateFoodListing(int userId, int foodId, [FromBody] JObject data)
        {
            var food = await _service.UpdateFoodListing(userId, foodId, data);
            return Ok(food);
        }

        [HttpPut("food/{foodId:int}/close")]
        public async Task<IActionResult> CloseFoodListing(int userId, int foodId)
        {
            var food = await _service.CloseFoodListing(userId, foodId);
            return Ok(food);
        }

        [HttpGet("food/{foodId:int}/images")]
        public async Task<IActionResult> GetFoodImages(int userId, int foodId)
        {
            var images = await _service.GetFoodImages(userId, foodId);
            return Ok(images);
        }


        [HttpPost("food/{foodId:int}/images")]
        public async Task<IActionResult> AddFoodImage(int userId, int foodId, [FromForm] IFormFile file)
        {
            var image = await _service.AddFoodImage(userId, foodId, file);
            return Ok(image);
        }
        //[HttpPost("food/{foodId:int}/images")]
        //public async Task<IActionResult> AddFoodImage(int userId, int foodId, [FromBody] JObject data)
        //{
        //    var image = await _service.AddFoodImage(userId, foodId, data);
        //    return Ok(image);
        //}

        [HttpDelete("food/{foodId:int}/images/{imageId:int}")]
        public async Task<IActionResult> DeleteFoodImage(int userId, int foodId, int imageId)
        {
            await _service.DeleteFoodImage(userId, foodId, imageId);
            return Ok(new { Message = "Image record deleted." });
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

        [HttpPut("requests/{requestId:int}/approve")]
        public async Task<IActionResult> ApprovePickupRequest(int userId, int requestId)
        {
            var request = await _service.ApprovePickupRequest(userId, requestId);
            return Ok(request);
        }

        [HttpPut("requests/{requestId:int}/reject")]
        public async Task<IActionResult> RejectPickupRequest(int userId, int requestId)
        {
            var request = await _service.RejectPickupRequest(userId, requestId);
            return Ok(request);
        }

        [HttpPut("requests/{requestId:int}/collect")]
        public async Task<IActionResult> ConfirmPickup(int userId, int requestId, [FromBody] JObject data)
        {
            var pickup = await _service.ConfirmPickup(userId, requestId, data);
            return Ok(pickup);
        }

        [HttpGet("pickups")]
        public async Task<IActionResult> GetPickupHistory(int userId)
        {
            var pickups = await _service.GetPickupHistory(userId);
            return Ok(pickups);
        }
    }
}
