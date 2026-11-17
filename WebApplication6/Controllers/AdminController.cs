using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebApplication6.Service;

namespace WebApplication6.Controllers
{
    [ApiController]
    [Route("Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ServiceAdmin _service;

        public AdminController(ServiceAdmin service)
        {
            _service = service;
        }

        [HttpPost("createuser")]
        public async Task<IActionResult> CreateUser(int userId, [FromBody] JObject data)
        {
            var user = await _service.CreateUser(userId, data);
            return Ok(user);
        }

        [HttpPost("setup")]
        public async Task<IActionResult> SetupAdmin([FromBody] JObject data)
        {
            var admin = await _service.SetupAdmin(data);
            return Ok(admin);
        }

        [HttpGet("organizations")]
        public async Task<IActionResult> GetOrganizations(int userId)
        {
            var organizations = await _service.GetOrganizations(userId);
            return Ok(organizations);
        }

        [HttpGet("organizations/{organizationId:int}")]
        public async Task<IActionResult> GetOrganizationById(int userId, int organizationId)
        {
            var organization = await _service.GetOrganizationById(userId, organizationId);
            return Ok(organization);
        }

        [HttpPost("organizations")]
        public async Task<IActionResult> AddOrganization(int userId, [FromBody] JObject data)
        {
            var organization = await _service.AddOrganization(userId, data);
            return Ok(organization);
        }

        [HttpPut("organizations/{organizationId:int}")]
        public async Task<IActionResult> UpdateOrganization(int userId, int organizationId, [FromBody] JObject data)
        {
            var organization = await _service.UpdateOrganization(userId, organizationId, data);
            return Ok(organization);
        }

        [HttpDelete("organizations/{organizationId:int}")]
        public async Task<IActionResult> DeleteOrganization(int userId, int organizationId)
        {
            await _service.DeleteOrganization(userId, organizationId);
            return Ok(new { Message = "Organization deleted." });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(int userId)
        {
            var users = await _service.GetUsers(userId);
            return Ok(users);
        }

        [HttpGet("users/{targetUserId:int}")]
        public async Task<IActionResult> GetUserById(int userId, int targetUserId)
        {
            var user = await _service.GetUserById(userId, targetUserId);
            return Ok(user);
        }

        [HttpPost("users")]
        public async Task<IActionResult> AddUser(int userId, [FromBody] JObject data)
        {
            var user = await _service.AddUser(userId, data);
            return Ok(user);
        }

        [HttpPut("users/{targetUserId:int}")]
        public async Task<IActionResult> UpdateUser(int userId, int targetUserId, [FromBody] JObject data)
        {
            var user = await _service.UpdateUser(userId, targetUserId, data);
            return Ok(user);
        }

        [HttpDelete("users/{targetUserId:int}")]
        public async Task<IActionResult> DeleteUser(int userId, int targetUserId)
        {
            await _service.DeleteUser(userId, targetUserId);
            return Ok(new { Message = "User deleted." });
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetFoodCategories(int userId)
        {
            var categories = await _service.GetFoodCategories(userId);
            return Ok(categories);
        }

        [HttpGet("categories/{categoryId:int}")]
        public async Task<IActionResult> GetFoodCategoryById(int userId, int categoryId)
        {
            var category = await _service.GetFoodCategoryById(userId, categoryId);
            return Ok(category);
        }

        [HttpPost("categories")]
        public async Task<IActionResult> AddFoodCategory(int userId, [FromBody] JObject data)
        {
            var category = await _service.AddFoodCategory(userId, data);
            return Ok(category);
        }

        [HttpPut("categories/{categoryId:int}")]
        public async Task<IActionResult> UpdateFoodCategory(int userId, int categoryId, [FromBody] JObject data)
        {
            var category = await _service.UpdateFoodCategory(userId, categoryId, data);
            return Ok(category);
        }

        [HttpDelete("categories/{categoryId:int}")]
        public async Task<IActionResult> DeleteFoodCategory(int userId, int categoryId)
        {
            await _service.DeleteFoodCategory(userId, categoryId);
            return Ok(new { Message = "Category deleted." });
        }
    }
}
