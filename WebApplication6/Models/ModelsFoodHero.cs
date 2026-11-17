#nullable enable

namespace WebApplication6.Models
{
    public class Organizations
    {
        public int OrganizationId { get; set; } = 0;
        public string OrganizationName { get; set; } = string.Empty;
        public string OrganizationType { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ICollection<Users> Users { get; set; } = new List<Users>();
        public virtual ICollection<FoodListings> FoodListings { get; set; } = new List<FoodListings>();
        public virtual ICollection<PickupRequests> PickupRequests { get; set; } = new List<PickupRequests>();
    }

    public class Users
    {
        public int UserId { get; set; } = 0;
        public int OrganizationId { get; set; } = 0;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AuthSubject { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? Password { get; set; }
        public virtual ICollection<FoodListings> FoodListings { get; set; } = new List<FoodListings>();
        public virtual ICollection<PickupRequests> PickupRequests { get; set; } = new List<PickupRequests>();
        public virtual ICollection<Pickups> Pickups { get; set; } = new List<Pickups>();
        public virtual Organizations? Organizations { get; set; }
    }

    public class FoodCategories
    {
        public int CategoryId { get; set; } = 0;
        public string CategoryName { get; set; } = string.Empty;
        public virtual ICollection<FoodListings> FoodListings { get; set; } = new List<FoodListings>();
    }

    public class FoodListings
    {
        public int FoodId { get; set; } = 0;
        public int SchoolOrganizationId { get; set; } = 0;
        public int CreatedByUserId { get; set; } = 0;
        public int CategoryId { get; set; } = 0;
        public string FoodName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int QuantityAvailable { get; set; } = 0;
        public string Unit { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public DateTime AvailableFrom { get; set; } = DateTime.Now;
        public DateTime CollectBy { get; set; } = DateTime.Now;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ICollection<FoodImages> FoodImages { get; set; } = new List<FoodImages>();
        public virtual ICollection<PickupRequests> PickupRequests { get; set; } = new List<PickupRequests>();
        public virtual Organizations? Organizations { get; set; }
        public virtual Users? Users { get; set; }
        public virtual FoodCategories? FoodCategories { get; set; }
    }

    public class FoodImages
    {
        public int ImageId { get; set; } = 0;
        public int FoodId { get; set; } = 0;
        public string ImageObjectKey { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public virtual FoodListings? FoodListings { get; set; }
    }

    public class PickupRequests
    {
        public int RequestId { get; set; } = 0;
        public int FoodId { get; set; } = 0;
        public int RecipientOrganizationId { get; set; } = 0;
        public int RequestedByUserId { get; set; } = 0;
        public int QuantityRequested { get; set; } = 0;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public virtual ICollection<Pickups> Pickups { get; set; } = new List<Pickups>();
        public virtual Organizations? Organizations { get; set; }
        public virtual Users? Users { get; set; }
        public virtual FoodListings? FoodListings { get; set; }
    }

    public class Pickups
    {
        public int PickupId { get; set; } = 0;
        public int RequestId { get; set; } = 0;
        public int ConfirmedByUserId { get; set; } = 0;
        public DateTime CollectedAt { get; set; } = DateTime.Now;
        public string? Notes { get; set; }
        public virtual Users? Users { get; set; }
        public virtual PickupRequests? PickupRequests { get; set; }
    }

}