using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net.Mail;
using WebApplication6.Models;

namespace WebApplication6.Service
{
    public class ServiceAdmin
    {
        private readonly MyDbContextFoodHero _context;

        public ServiceAdmin(MyDbContextFoodHero context)
        {
            _context = context;
        }

        public async Task<Users> CreateUser(int userId, JObject data)
        {
            await GetAdmin(userId);
            var user = data.ToObject<Users>()!;
            user.Role = "ADMIN";

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<object> SetupAdmin(JObject data)
        {
            var organization = data.ToObject<Organizations>()!;
            var admin = data.ToObject<Users>()!;
            admin.Role = "ADMIN";

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            admin.OrganizationId = organization.OrganizationId;
            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            return new { organization.OrganizationId, admin.UserId, admin.FullName, admin.Email, admin.Role };
        }

        public async Task<List<Organizations>> GetOrganizations(int userId)
        {
            await GetAdmin(userId);
            return await _context.Organizations.AsNoTracking().ToListAsync();
        }

        public async Task<Organizations> GetOrganizationById(int userId, int organizationId)
        {
            await GetAdmin(userId);
            var organization = await _context.Organizations.AsNoTracking()
                .SingleOrDefaultAsync(o => o.OrganizationId == organizationId);

            return organization ?? throw new KeyNotFoundException("Organization was not found.");
        }

        public async Task<Organizations> AddOrganization(int userId, JObject data)
        {
            await GetAdmin(userId);
            var organization = data.ToObject<Organizations>()!;
            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<Organizations> UpdateOrganization(int userId, int organizationId, JObject data)
        {
            await GetAdmin(userId);
            var organization = await _context.Organizations
                .SingleOrDefaultAsync(o => o.OrganizationId == organizationId)
                ?? throw new KeyNotFoundException("Organization was not found.");

            var updated = data.ToObject<Organizations>()!;
            organization.OrganizationName = updated.OrganizationName;
            organization.OrganizationType = updated.OrganizationType;
            organization.Address = updated.Address;
            organization.Phone = updated.Phone;

            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task DeleteOrganization(int userId, int organizationId)
        {
            await GetAdmin(userId);
            var organization = await _context.Organizations
                .SingleOrDefaultAsync(o => o.OrganizationId == organizationId)
                ?? throw new KeyNotFoundException("Organization was not found.");

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
        }

        // ---------------- Users ----------------

        public async Task<List<Users>> GetUsers(int userId)
        {
            await GetAdmin(userId);
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        public async Task<Users> GetUserById(int userId, int targetUserId)
        {
            await GetAdmin(userId);
            var user = await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == targetUserId);

            return user ?? throw new KeyNotFoundException("User was not found.");
        }

        public async Task<Users> AddUser(int userId, JObject data)
        {
            await GetAdmin(userId);
            var user = data.ToObject<Users>()!;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Users> UpdateUser(int userId, int targetUserId, JObject data)
        {
            await GetAdmin(userId);
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.UserId == targetUserId)
                ?? throw new KeyNotFoundException("User was not found.");

            var updated = data.ToObject<Users>()!;
            user.OrganizationId = updated.OrganizationId;
            user.FullName = updated.FullName;
            user.Email = updated.Email;
            user.AuthSubject = updated.AuthSubject;
            user.Role = updated.Role;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteUser(int userId, int targetUserId)
        {
            await GetAdmin(userId);
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.UserId == targetUserId)
                ?? throw new KeyNotFoundException("User was not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        // ---------------- Food categories ----------------

        public async Task<List<FoodCategories>> GetFoodCategories(int userId)
        {
            await GetAdmin(userId);
            return await _context.FoodCategories.AsNoTracking().ToListAsync();
        }

        public async Task<FoodCategories> GetFoodCategoryById(int userId, int categoryId)
        {
            await GetAdmin(userId);
            var category = await _context.FoodCategories.AsNoTracking()
                .SingleOrDefaultAsync(c => c.CategoryId == categoryId);

            return category ?? throw new KeyNotFoundException("Category was not found.");
        }

        public async Task<FoodCategories> AddFoodCategory(int userId, JObject data)
        {
            await GetAdmin(userId);
            var category = data.ToObject<FoodCategories>()!;
            _context.FoodCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<FoodCategories> UpdateFoodCategory(int userId, int categoryId, JObject data)
        {
            await GetAdmin(userId);
            var category = await _context.FoodCategories
                .SingleOrDefaultAsync(c => c.CategoryId == categoryId)
                ?? throw new KeyNotFoundException("Category was not found.");

            var updated = data.ToObject<FoodCategories>()!;
            category.CategoryName = updated.CategoryName;

            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteFoodCategory(int userId, int categoryId)
        {
            await GetAdmin(userId);
            var category = await _context.FoodCategories
                .SingleOrDefaultAsync(c => c.CategoryId == categoryId)
                ?? throw new KeyNotFoundException("Category was not found.");

            _context.FoodCategories.Remove(category);
            await _context.SaveChangesAsync();
        }

        // ---------------- Helper ----------------

        private async Task<Users> GetAdmin(int userId)
        {
            var admin = await _context.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == userId && u.Role == "ADMIN");

            return admin ?? throw new UnauthorizedAccessException("This API requires an ADMIN user.");
        }
    }
}
