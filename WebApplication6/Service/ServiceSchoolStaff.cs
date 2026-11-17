using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Data;
using WebApplication6.Models;

namespace WebApplication6.Service
{
    public class ServiceSchoolStaff
    {
        private readonly MyDbContextFoodHero _context;

        public ServiceSchoolStaff(MyDbContextFoodHero context)
        {
            _context = context;
        }

        public async Task<List<Organizations>> GetOrganizations()
        {
            return await _context.Organizations
                .AsNoTracking()
                .OrderBy(o => o.OrganizationName)
                .ToListAsync();
        }
        public async Task<Users> CreateUser(JObject data)
        {
            var user = data.ToObject<Users>()!;
            user.Role = "SCHOOL_STAFF";

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<Users>> GetUsers()
        {
            return await _context.Users.AsNoTracking()
                .Where(u => u.Role == "SCHOOL_STAFF").ToListAsync();
        }

        public async Task<Users> GetUserById(int userId)
        {
            return await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == userId && u.Role == "SCHOOL_STAFF")
                ?? throw new KeyNotFoundException("School staff user was not found.");
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
            await GetSchoolStaff(userId);
            return await _context.FoodCategories.AsNoTracking().ToListAsync();
        }

        public async Task<List<FoodListings>> GetAllFoodListings(int userId)
        {
            var staff = await GetSchoolStaff(userId);
            return await _context.FoodListings.AsNoTracking()
                .Where(f => f.SchoolOrganizationId == staff.OrganizationId)
                .ToListAsync();
        }

        public async Task<FoodListings> GetFoodListingById(int userId, int foodId)
        {
            var staff = await GetSchoolStaff(userId);
            return await GetSchoolFood(staff.OrganizationId, foodId);
        }

        public async Task<FoodListings> AddFoodListing(int userId, JObject data)
        {
            var staff = await GetSchoolStaff(userId);
            var food = data.ToObject<FoodListings>()!;

            food.SchoolOrganizationId = staff.OrganizationId;
            food.CreatedByUserId = staff.UserId;
            food.Status = "AVAILABLE";
            food.CreatedAt = DateTime.UtcNow;

            _context.FoodListings.Add(food);
            await _context.SaveChangesAsync();
            return food;
        }

        public async Task<FoodListings> UpdateFoodListing(int userId, int foodId, JObject data)
        {
            var staff = await GetSchoolStaff(userId);
            var food = await GetSchoolFood(staff.OrganizationId, foodId);

            if (food.Status != "AVAILABLE")
                throw new ArgumentException("Only an AVAILABLE listing can be edited.");

            var hasReserved = await _context.PickupRequests.AnyAsync(r =>
                r.FoodId == foodId && (r.Status == "APPROVED" || r.Status == "COLLECTED"));
            if (hasReserved)
                throw new ArgumentException("A listing with approved or collected requests cannot be edited.");

            var updated = data.ToObject<FoodListings>()!;
            _context.FoodListings.Attach(food);

            food.CategoryId = updated.CategoryId;
            food.FoodName = updated.FoodName;
            food.Description = updated.Description;
            food.QuantityAvailable = updated.QuantityAvailable;
            food.Unit = updated.Unit;
            food.PickupAddress = updated.PickupAddress;
            food.AvailableFrom = updated.AvailableFrom;
            food.CollectBy = updated.CollectBy;

            await _context.SaveChangesAsync();
            return food;
        }

        public async Task<FoodListings> CloseFoodListing(int userId, int foodId)
        {
            var staff = await GetSchoolStaff(userId);
            var food = await GetSchoolFood(staff.OrganizationId, foodId);

            var hasApproved = await _context.PickupRequests
                .AnyAsync(r => r.FoodId == foodId && r.Status == "APPROVED");
            if (hasApproved)
                throw new ArgumentException("Complete or cancel approved requests before closing this listing.");

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            await _context.PickupRequests
                .Where(r => r.FoodId == foodId && r.Status == "PENDING")
                .ExecuteUpdateAsync(u => u.SetProperty(r => r.Status, "REJECTED"));

            await _context.FoodListings
                .Where(f => f.FoodId == foodId && f.SchoolOrganizationId == staff.OrganizationId)
                .ExecuteUpdateAsync(u => u.SetProperty(f => f.Status, "CLOSED"));

            await transaction.CommitAsync();
            food.Status = "CLOSED";
            return food;
        }

        public async Task<List<FoodImages>> GetFoodImages(int userId, int foodId)
        {
            var staff = await GetSchoolStaff(userId);
            await GetSchoolFood(staff.OrganizationId, foodId);
            return await _context.FoodImages.AsNoTracking()
                .Where(i => i.FoodId == foodId).ToListAsync();
        }

        //public async Task<FoodImages> AddFoodImage(int userId, int foodId, JObject data)
        //{
        //    var staff = await GetSchoolStaff(userId);
        //    var food = await GetSchoolFood(staff.OrganizationId, foodId);

        //    if (food.Status != "AVAILABLE")
        //        throw new ArgumentException("Images can only be added to an AVAILABLE listing.");

        //    var image = new FoodImages
        //    {
        //        FoodId = foodId,
        //        ImageObjectKey = data.Value<string>("ImageObjectKey")?.Trim(),
        //        UploadedAt = DateTime.UtcNow
        //    };

        //    _context.FoodImages.Add(image);
        //    await _context.SaveChangesAsync();
        //    return image;
        //}
        public async Task<FoodImages> AddFoodImage(int userId, int foodId, IFormFile file)
        {
            var staff = await GetSchoolStaff(userId);
            var food = await GetSchoolFood(staff.OrganizationId, foodId);

            if (food.Status != "AVAILABLE")
                throw new ArgumentException("Images can only be added to an AVAILABLE listing.");

            if (file is null || file.Length == 0)
                throw new ArgumentException("No image file was uploaded.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{ext}";

            // project root /Image/foods/{foodId}/
            var folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Image", "foods", foodId.ToString());

            Directory.CreateDirectory(folder);

            var fullPath = Path.Combine(folder, fileName);
            using (var stream = File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var image = new FoodImages
            {
                FoodId = foodId,
                ImageObjectKey = $"/Image/foods/{foodId}/{fileName}",
                UploadedAt = DateTime.UtcNow
            };

            _context.FoodImages.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }
        public async Task DeleteFoodImage(int userId, int foodId, int imageId)
        {
            var staff = await GetSchoolStaff(userId);
            await GetSchoolFood(staff.OrganizationId, foodId);

            var image = await _context.FoodImages
                .SingleOrDefaultAsync(i => i.ImageId == imageId && i.FoodId == foodId)
                ?? throw new KeyNotFoundException("Image was not found in your school's listings.");

            _context.FoodImages.Remove(image);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PickupRequests>> GetPickupRequests(int userId)
        {
            var staff = await GetSchoolStaff(userId);
            return await SchoolRequests(staff.OrganizationId).ToListAsync();
        }

        public async Task<PickupRequests> GetPickupRequestById(int userId, int requestId)
        {
            var staff = await GetSchoolStaff(userId);
            return await GetSchoolRequest(staff.OrganizationId, requestId);
        }

        public async Task<PickupRequests> ApprovePickupRequest(int userId, int requestId)
        {
            var staff = await GetSchoolStaff(userId);
            var request = await GetSchoolRequest(staff.OrganizationId, requestId);

            if (request.Status == "APPROVED")
                return request;
            if (request.Status != "PENDING")
                throw new ArgumentException("Only a PENDING request can be approved.");

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var now = DateTime.UtcNow;
            var reserved = await _context.FoodListings
                .Where(f => f.FoodId == request.FoodId && f.SchoolOrganizationId == staff.OrganizationId
                    && f.Status == "AVAILABLE" && f.CollectBy > now
                    && f.QuantityAvailable >= request.QuantityRequested)
                .ExecuteUpdateAsync(u => u.SetProperty(
                    f => f.QuantityAvailable, f => f.QuantityAvailable - request.QuantityRequested));
            if (reserved == 0)
                throw new ArgumentException("The listing is closed, expired, or has insufficient food.");

            var approved = await _context.PickupRequests
                .Where(r => r.RequestId == requestId && r.Status == "PENDING")
                .ExecuteUpdateAsync(u => u.SetProperty(r => r.Status, "APPROVED"));
            if (approved == 0)
                throw new ArgumentException("The request changed. Please reload it before approving.");

            await transaction.CommitAsync();
            request.Status = "APPROVED";
            return request;
        }

        public async Task<PickupRequests> RejectPickupRequest(int userId, int requestId)
        {
            var staff = await GetSchoolStaff(userId);
            var request = await GetSchoolRequest(staff.OrganizationId, requestId);

            if (request.Status == "REJECTED")
                return request;
            if (request.Status != "PENDING")
                throw new ArgumentException("Only a PENDING request can be rejected.");

            var rejected = await _context.PickupRequests
                .Where(r => r.RequestId == requestId && r.Status == "PENDING")
                .ExecuteUpdateAsync(u => u.SetProperty(r => r.Status, "REJECTED"));
            if (rejected == 0)
                throw new ArgumentException("The request changed. Please reload it before rejecting.");

            request.Status = "REJECTED";
            return request;
        }

        public async Task<Pickups> ConfirmPickup(int userId, int requestId, JObject data)
        {
            var staff = await GetSchoolStaff(userId);
            var request = await GetSchoolRequest(staff.OrganizationId, requestId);
            var food = await GetSchoolFood(staff.OrganizationId, request.FoodId);

            var existing = await _context.Pickups.AsNoTracking()
                .SingleOrDefaultAsync(p => p.RequestId == requestId);
            if (request.Status == "COLLECTED" && existing != null)
                return existing;
            if (request.Status != "APPROVED" || existing != null)
                throw new ArgumentException("Only an APPROVED request without a pickup record can be collected.");

            var now = DateTime.UtcNow;
            if (food.Status != "AVAILABLE" || now < food.AvailableFrom || now >= food.CollectBy)
                throw new ArgumentException("Collection must happen during the listing's pickup window.");

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var collected = await _context.PickupRequests
                .Where(r => r.RequestId == requestId && r.Status == "APPROVED")
                .ExecuteUpdateAsync(u => u.SetProperty(r => r.Status, "COLLECTED"));
            if (collected == 0)
                throw new ArgumentException("The request changed. Please reload it before confirming collection.");

            var pickup = new Pickups
            {
                RequestId = requestId,
                ConfirmedByUserId = staff.UserId,
                CollectedAt = now,
                Notes = data.Value<string>("Notes")?.Trim()
            };
            _context.Pickups.Add(pickup);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return pickup;
        }

        public async Task<List<Pickups>> GetPickupHistory(int userId)
        {
            var staff = await GetSchoolStaff(userId);
            var requestIds = SchoolRequests(staff.OrganizationId).Select(r => r.RequestId);
            return await _context.Pickups.AsNoTracking()
                .Where(p => requestIds.Contains(p.RequestId))
                .ToListAsync();
        }

        // ---------------- Helpers ----------------

        private async Task<Users> GetSchoolStaff(int userId)
        {
            var staff = await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == userId && u.Role == "SCHOOL_STAFF"
                    && _context.Organizations.Any(o => o.OrganizationId == u.OrganizationId
                        && o.OrganizationType == "SCHOOL"));

            return staff ?? throw new UnauthorizedAccessException(
                "This API requires a SCHOOL_STAFF user belonging to a SCHOOL organization.");
        }

        private async Task<FoodListings> GetSchoolFood(int organizationId, int foodId)
        {
            var food = await _context.FoodListings.AsNoTracking()
                .SingleOrDefaultAsync(f => f.FoodId == foodId && f.SchoolOrganizationId == organizationId);

            return food ?? throw new KeyNotFoundException("Food listing was not found in your school.");
        }

        private IQueryable<PickupRequests> SchoolRequests(int organizationId)
        {
            return _context.PickupRequests.AsNoTracking()
                .Where(r => _context.FoodListings.Any(f =>
                    f.FoodId == r.FoodId && f.SchoolOrganizationId == organizationId));
        }

        private async Task<PickupRequests> GetSchoolRequest(int organizationId, int requestId)
        {
            var request = await SchoolRequests(organizationId)
                .SingleOrDefaultAsync(r => r.RequestId == requestId);

            return request ?? throw new KeyNotFoundException("Pickup request was not found in your school.");
        }
    }
}
