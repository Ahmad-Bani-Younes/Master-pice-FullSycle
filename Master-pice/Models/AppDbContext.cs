using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Master_pice.ViewModel;

namespace Master_pice.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Restrict); // ✅ منع الـ Cascade

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Laptop> Laptops { get; set; }
        public DbSet<PC> PCs { get; set; }
        public DbSet<PCPart> PCParts { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<CartDetail> CartDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Master_pice.ViewModel.ProductViewModel> ProductViewModel { get; set; } = default!;
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<AboutContent> AboutContents { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        public DbSet<Fav> Favorites { get; set; }

        public DbSet<Company> Companies { get; set; }



    }
}
