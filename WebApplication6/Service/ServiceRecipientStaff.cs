using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Data;
using WebApplication6.Models;

namespace WebApplication6.Service
{
    public class ServiceRecipientStaff
    {
        private readonly MyDbContextFoodHero _context;

        public ServiceRecipientStaff(MyDbContextFoodHero context)
        {
            _context = context;
        }

        public async Task<Users> CreateUser(JObject data)
        {
            var user = data.ToObject<Users>()!;
            user.Role = "RECIPIENT_STAFF";

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<Users>> GetUsers()
        {
            return await _context.Users.AsNoTracking()
                .Where(u => u.Role == "RECIPIENT_STAFF").ToListAsync();
        }

        public async Task<Users> GetUserById(int userId)
        {
            return await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == userId && u.Role == "RECIPIENT_STAFF")
                ?? throw new KeyNotFoundException("Recipient staff user was not found.");
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

        public async Task<List<FoodCategories>> GetFoodCategories(int userId)
        {
            await GetRecipientStaff(userId);
            return await _context.FoodCategories.AsNoTracking().ToListAsync();
        }

        public async Task<List<FoodListings>> GetAvailableFood(int userId)
        {
            await GetRecipientStaff(userId);
            var now = DateTime.UtcNow;
            return await _context.FoodListings.AsNoTracking()
                .Where(f => f.Status == "AVAILABLE" && f.QuantityAvailable > 0 && f.CollectBy > now)
                .ToListAsync();
        }

        public async Task<FoodListings> GetFoodById(int userId, int foodId)
        {
            await GetRecipientStaff(userId);
            var now = DateTime.UtcNow;
            var food = await _context.FoodListings.AsNoTracking()
                .SingleOrDefaultAsync(f => f.FoodId == foodId && f.Status == "AVAILABLE"
                    && f.QuantityAvailable > 0 && f.CollectBy > now);

            return food ?? throw new KeyNotFoundException("Available food listing was not found.");
        }

        public async Task<List<FoodImages>> GetFoodImages(int userId, int foodId)
        {
            await GetFoodById(userId, foodId);
            return await _context.FoodImages.AsNoTracking()
                .Where(i => i.FoodId == foodId).ToListAsync();
        }

        public async Task<PickupRequests> AddPickupRequest(int userId, JObject data)
        {
            var staff = await GetRecipientStaff(userId);
            var request = data.ToObject<PickupRequests>()!;

            if (request.QuantityRequested <= 0)
                throw new ArgumentException("QuantityRequested must be positive.");

            var now = DateTime.UtcNow;
            var available = await _context.FoodListings.AnyAsync(f => f.FoodId == request.FoodId
                && f.Status == "AVAILABLE" && f.CollectBy > now
                && f.QuantityAvailable >= request.QuantityRequested);
            if (!available)
                throw new ArgumentException("The listing is closed, expired, or has insufficient food.");

            request.RecipientOrganizationId = staff.OrganizationId;
            request.RequestedByUserId = staff.UserId;
            request.Status = "PENDING";
            request.RequestedAt = now;

            _context.PickupRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<List<PickupRequests>> GetPickupRequests(int userId)
        {
            var staff = await GetRecipientStaff(userId);
            return await _context.PickupRequests.AsNoTracking()
                .Where(r => r.RecipientOrganizationId == staff.OrganizationId)
                .ToListAsync();
        }

        public async Task<PickupRequests> GetPickupRequestById(int userId, int requestId)
        {
            var staff = await GetRecipientStaff(userId);
            var request = await _context.PickupRequests.AsNoTracking()
                .SingleOrDefaultAsync(r => r.RequestId == requestId
                    && r.RecipientOrganizationId == staff.OrganizationId);

            return request ?? throw new KeyNotFoundException("Pickup request was not found in your organization.");
        }

        public async Task<PickupRequests> CancelPickupRequest(int userId, int requestId)
        {
            var staff = await GetRecipientStaff(userId);
            var request = await _context.PickupRequests.AsNoTracking()
                .SingleOrDefaultAsync(r => r.RequestId == requestId
                    && r.RecipientOrganizationId == staff.OrganizationId)
                ?? throw new KeyNotFoundException("Pickup request was not found in your organization.");

            if (request.Status == "CANCELLED")
                return request;
            if (request.Status != "PENDING" && request.Status != "APPROVED")
                throw new ArgumentException("Only a PENDING or APPROVED request can be cancelled.");

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var previousStatus = request.Status;
            var changed = await _context.PickupRequests
                .Where(r => r.RequestId == requestId
                    && r.RecipientOrganizationId == staff.OrganizationId
                    && r.Status == previousStatus)
                .ExecuteUpdateAsync(u => u.SetProperty(r => r.Status, "CANCELLED"));
            if (changed == 0)
                throw new ArgumentException("The request changed. Please reload it before cancelling.");

            if (previousStatus == "APPROVED")
            {
                await _context.FoodListings
                    .Where(f => f.FoodId == request.FoodId
                        && f.QuantityAvailable <= int.MaxValue - request.QuantityRequested)
                    .ExecuteUpdateAsync(u => u.SetProperty(
                        f => f.QuantityAvailable, f => f.QuantityAvailable + request.QuantityRequested));
            }

            await transaction.CommitAsync();
            request.Status = "CANCELLED";
            return request;
        }

        public async Task<List<Pickups>> GetPickupHistory(int userId)
        {
            var staff = await GetRecipientStaff(userId);
            var requestIds = _context.PickupRequests
                .Where(r => r.RecipientOrganizationId == staff.OrganizationId)
                .Select(r => r.RequestId);

            return await _context.Pickups.AsNoTracking()
                .Where(p => requestIds.Contains(p.RequestId))
                .ToListAsync();
        }

        private async Task<Users> GetRecipientStaff(int userId)
        {
            var staff = await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == userId && u.Role == "RECIPIENT_STAFF"
                    && _context.Organizations.Any(o => o.OrganizationId == u.OrganizationId
                        && (o.OrganizationType == "CHARITY" || o.OrganizationType == "FOOD_BANK"
                            || o.OrganizationType == "COMMUNITY")));

            return staff ?? throw new UnauthorizedAccessException(
                "This API requires RECIPIENT_STAFF from a CHARITY, FOOD_BANK, or COMMUNITY organization.");
        }
    }
}
