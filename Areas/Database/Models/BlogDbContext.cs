using blog.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace blog.Areas.Database.Models
{
    public class BlogDbContext : IdentityDbContext<BlogUser>
    {


        public DbSet<Category> Categories { set; get; }
        public DbSet<Post> Posts { set; get; }
        public DbSet<PostCategory> PostCategories { set; get; }


        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName[6..]);
                }
            }

            // Tạo Index cho cột Slug bảng Category
            builder.Entity<Category>(entity =>
            {
                entity.HasIndex(p => p.Slug).IsUnique();
            });


            // Tạo key của bảng là sự kết hợp PostID, CategoryID, qua đó
            // tạo quan hệ many to many giữa Post và Category
            builder.Entity<PostCategory>().HasKey(p => new { p.PostID, p.CategoryID });

            builder.Entity<Post>(entity =>
            {
                entity.HasIndex(p => p.Slug)
                      .IsUnique();
            });
        }

    }

}