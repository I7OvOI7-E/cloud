using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using WebApplication6.Models;

namespace WebApplication6.Service
{
    public class ServiceVolunteer
    {
        private readonly MyDbContextFoodHero _context;

        public ServiceVolunteer(MyDbContextFoodHero context)
        {
            _context = context;
        }

        public async Task<Users> CreateUser(JObject data)
        {
            var user = data.ToObject<Users>()!;
            user.Role = "VOLUNTEER";

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<Users>> GetUsers()
        {
            return await _context.Users.AsNoTracking()
                .Where(u => u.Role == "VOLUNTEER").ToListAsync();
        }

        public async Task<Users> GetUserById(int userId)
        {
            return await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == userId && u.Role == "VOLUNTEER")
                ?? throw new KeyNotFoundException("Volunteer user was not found.");
        }

        public async Task<Users> UpdateUser(int userId, JObject data)
        {
            var existingUser = await GetUserById(userId);
            var user = data.ToObject<Users>()!;
            user.UserId = userId;
            user.Role = existingUser.Role;
            user.CreatedAt = existingUser.CreatedAt;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteUser(int userId)
        {
            var user = await GetUserById(userId);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<object>> GetCollections(int userId)
        {
            var volunteer = await GetVolunteer(userId);
            var now = DateTime.UtcNow;

            var collections = await (
                from request in _context.PickupRequests.AsNoTracking()
                join food in _context.FoodListings on request.FoodId equals food.FoodId
                where request.Status == "APPROVED" && food.Status == "AVAILABLE" && food.CollectBy > now
                    && (food.SchoolOrganizationId == volunteer.OrganizationId
                        || request.RecipientOrganizationId == volunteer.OrganizationId)
                select new
                {
                    request.RequestId,
                    food.FoodId,
                    food.FoodName,
                    request.QuantityRequested,
                    food.Unit,
                    food.PickupAddress,
                    food.AvailableFrom,
                    food.CollectBy,
                    food.SchoolOrganizationId,
                    request.RecipientOrganizationId
                }).ToListAsync();

            return collections.Cast<object>().ToList();
        }

        public async Task<object> GetCollectionById(int userId, int requestId)
        {
            var volunteer = await GetVolunteer(userId);
            var now = DateTime.UtcNow;

            var collection = await (
                from request in _context.PickupRequests.AsNoTracking()
                join food in _context.FoodListings on request.FoodId equals food.FoodId
                where request.RequestId == requestId && request.Status == "APPROVED"
                    && food.Status == "AVAILABLE" && food.CollectBy > now
                    && (food.SchoolOrganizationId == volunteer.OrganizationId
                        || request.RecipientOrganizationId == volunteer.OrganizationId)
                select new
                {
                    request.RequestId,
                    food.FoodId,
                    food.FoodName,
                    request.QuantityRequested,
                    food.Unit,
                    food.PickupAddress,
                    food.AvailableFrom,
                    food.CollectBy,
                    food.SchoolOrganizationId,
                    request.RecipientOrganizationId
                }).SingleOrDefaultAsync();

            return collection ?? throw new KeyNotFoundException("An approved collection was not found for your organization.");
        }

        public async Task<List<object>> GetPickupHistory(int userId)
        {
            var volunteer = await GetVolunteer(userId);

            var pickups = await (
                from pickup in _context.Pickups.AsNoTracking()
                join request in _context.PickupRequests on pickup.RequestId equals request.RequestId
                join food in _context.FoodListings on request.FoodId equals food.FoodId
                where food.SchoolOrganizationId == volunteer.OrganizationId
                    || request.RecipientOrganizationId == volunteer.OrganizationId
                select new
                {
                    pickup.PickupId,
                    pickup.RequestId,
                    food.FoodName,
                    request.QuantityRequested,
                    food.Unit,
                    food.PickupAddress,
                    pickup.CollectedAt,
                    pickup.Notes
                }).ToListAsync();

            return pickups.Cast<object>().ToList();
        }

        private async Task<Users> GetVolunteer(int userId)
        {
            var volunteer = await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == userId && u.Role == "VOLUNTEER");

            return volunteer ?? throw new UnauthorizedAccessException("This API requires a VOLUNTEER user.");
        }
    }
}
