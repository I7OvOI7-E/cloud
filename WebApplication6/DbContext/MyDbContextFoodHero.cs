using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication6.Models;

namespace WebApplication6
{
    public class MyDbContextFoodHero : DbContext
    {

        public MyDbContextFoodHero(DbContextOptions<MyDbContextFoodHero> options) : base(options)
        {
        }

        public DbSet<Organizations> Organizations { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<FoodCategories> FoodCategories { get; set; }
        public DbSet<FoodListings> FoodListings { get; set; }
        public DbSet<FoodImages> FoodImages { get; set; }
        public DbSet<PickupRequests> PickupRequests { get; set; }
        public DbSet<Pickups> Pickups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyDbContextFoodHero).Assembly);
    }
    }

    public class OrganizationsConfig : IEntityTypeConfiguration<Organizations>
    {
        public void Configure(EntityTypeBuilder<Organizations> builder)
        {
            builder.HasKey(e => e.OrganizationId);
            builder.Property(e => e.OrganizationId).ValueGeneratedOnAdd();
            builder.Property(e => e.OrganizationId).IsRequired();
            builder.Property(e => e.OrganizationName).HasMaxLength(255);
            builder.Property(e => e.OrganizationType).HasMaxLength(255);
            builder.Property(e => e.Address).HasMaxLength(255);
            builder.Property(e => e.Phone).HasMaxLength(255);
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.HasMany(e => e.Users)
                   .WithOne(e => e.Organizations)
                   .HasForeignKey(e => e.OrganizationId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(e => e.FoodListings)
                   .WithOne(e => e.Organizations)
                   .HasForeignKey(e => e.SchoolOrganizationId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(e => e.PickupRequests)
                   .WithOne(e => e.Organizations)
                   .HasForeignKey(e => e.RecipientOrganizationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class UsersConfig : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            builder.HasKey(e => e.UserId);
            builder.Property(e => e.UserId).ValueGeneratedOnAdd();
            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.OrganizationId).IsRequired();
            builder.Property(e => e.FullName).HasMaxLength(255);
            builder.Property(e => e.Email).HasMaxLength(255);
            builder.Property(e => e.AuthSubject).HasMaxLength(255);
            builder.Property(e => e.Role).HasMaxLength(255);
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.HasMany(e => e.FoodListings)
                   .WithOne(e => e.Users)
                   .HasForeignKey(e => e.CreatedByUserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(e => e.PickupRequests)
                   .WithOne(e => e.Users)
                   .HasForeignKey(e => e.RequestedByUserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(e => e.Pickups)
                   .WithOne(e => e.Users)
                   .HasForeignKey(e => e.ConfirmedByUserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(e => e.FoodListings)
                   .WithOne(e => e.Users)
                   .HasForeignKey(e => e.SchoolOrganizationId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(e => e.PickupRequests)
                   .WithOne(e => e.Users)
                   .HasForeignKey(e => e.RecipientOrganizationId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.Organizations)
                   .WithMany(e => e.Users)
                   .HasForeignKey(e => e.OrganizationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class FoodCategoriesConfig : IEntityTypeConfiguration<FoodCategories>
    {
        public void Configure(EntityTypeBuilder<FoodCategories> builder)
        {
            builder.HasKey(e => e.CategoryId);
            builder.Property(e => e.CategoryId).ValueGeneratedOnAdd();
            builder.Property(e => e.CategoryId).IsRequired();
            builder.Property(e => e.CategoryName).HasMaxLength(255);
            builder.HasMany(e => e.FoodListings)
                   .WithOne(e => e.FoodCategories)
                   .HasForeignKey(e => e.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class FoodListingsConfig : IEntityTypeConfiguration<FoodListings>
    {
        public void Configure(EntityTypeBuilder<FoodListings> builder)
        {
            builder.HasKey(e => e.FoodId);
            builder.Property(e => e.FoodId).ValueGeneratedOnAdd();
            builder.Property(e => e.FoodId).IsRequired();
            builder.Property(e => e.SchoolOrganizationId).IsRequired();
            builder.Property(e => e.CreatedByUserId).IsRequired();
            builder.Property(e => e.CategoryId).IsRequired();
            builder.Property(e => e.FoodName).HasMaxLength(255);
            builder.Property(e => e.Description).HasMaxLength(255);
            builder.Property(e => e.QuantityAvailable).IsRequired();
            builder.Property(e => e.Unit).HasMaxLength(255);
            builder.Property(e => e.PickupAddress).HasMaxLength(255);
            builder.Property(e => e.AvailableFrom).IsRequired();
            builder.Property(e => e.CollectBy).IsRequired();
            builder.Property(e => e.Status).HasMaxLength(255);
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.HasMany(e => e.FoodImages)
                   .WithOne(e => e.FoodListings)
                   .HasForeignKey(e => e.FoodId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(e => e.PickupRequests)
                   .WithOne(e => e.FoodListings)
                   .HasForeignKey(e => e.FoodId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.Organizations)
                   .WithMany(e => e.FoodListings)
                   .HasForeignKey(e => e.SchoolOrganizationId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.Users)
                   .WithMany(e => e.FoodListings)
                   .HasForeignKey(e => e.CreatedByUserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.Users)
                   .WithMany(e => e.FoodListings)
                   .HasForeignKey(e => e.SchoolOrganizationId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.FoodCategories)
                   .WithMany(e => e.FoodListings)
                   .HasForeignKey(e => e.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class FoodImagesConfig : IEntityTypeConfiguration<FoodImages>
    {
        public void Configure(EntityTypeBuilder<FoodImages> builder)
        {
            builder.HasKey(e => e.ImageId);
            builder.Property(e => e.ImageId).ValueGeneratedOnAdd();
            builder.Property(e => e.ImageId).IsRequired();
            builder.Property(e => e.FoodId).IsRequired();
            builder.Property(e => e.ImageObjectKey).HasMaxLength(255);
            builder.Property(e => e.UploadedAt).IsRequired();
            builder.HasOne(e => e.FoodListings)
                   .WithMany(e => e.FoodImages)
                   .HasForeignKey(e => e.FoodId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class PickupRequestsConfig : IEntityTypeConfiguration<PickupRequests>
    {
        public void Configure(EntityTypeBuilder<PickupRequests> builder)
        {
            builder.HasKey(e => e.RequestId);
            builder.Property(e => e.RequestId).ValueGeneratedOnAdd();
            builder.Property(e => e.RequestId).IsRequired();
            builder.Property(e => e.FoodId).IsRequired();
            builder.Property(e => e.RecipientOrganizationId).IsRequired();
            builder.Property(e => e.RequestedByUserId).IsRequired();
            builder.Property(e => e.QuantityRequested).IsRequired();
            builder.Property(e => e.Status).HasMaxLength(255);
            builder.Property(e => e.Notes).HasMaxLength(255);
            builder.Property(e => e.RequestedAt).IsRequired();
            builder.HasMany(e => e.Pickups)
                   .WithOne(e => e.PickupRequests)
                   .HasForeignKey(e => e.RequestId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.Organizations)
                   .WithMany(e => e.PickupRequests)
                   .HasForeignKey(e => e.RecipientOrganizationId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.Users)
                   .WithMany(e => e.PickupRequests)
                   .HasForeignKey(e => e.RequestedByUserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.Users)
                   .WithMany(e => e.PickupRequests)
                   .HasForeignKey(e => e.RecipientOrganizationId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.FoodListings)
                   .WithMany(e => e.PickupRequests)
                   .HasForeignKey(e => e.FoodId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class PickupsConfig : IEntityTypeConfiguration<Pickups>
    {
        public void Configure(EntityTypeBuilder<Pickups> builder)
        {
            builder.HasKey(e => e.PickupId);
            builder.Property(e => e.PickupId).ValueGeneratedOnAdd();
            builder.Property(e => e.PickupId).IsRequired();
            builder.Property(e => e.RequestId).IsRequired();
            builder.Property(e => e.ConfirmedByUserId).IsRequired();
            builder.Property(e => e.CollectedAt).IsRequired();
            builder.Property(e => e.Notes).HasMaxLength(255);
            builder.HasOne(e => e.Users)
                   .WithMany(e => e.Pickups)
                   .HasForeignKey(e => e.ConfirmedByUserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(e => e.PickupRequests)
                   .WithMany(e => e.Pickups)
                   .HasForeignKey(e => e.RequestId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

}