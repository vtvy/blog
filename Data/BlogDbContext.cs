using blog.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace blog.Data
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
            // Bỏ tiền tố AspNet của các bảng: mặc định
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
                entity.HasIndex(p => p.Slug);
            });


            // Tạo key của bảng là sự kết hợp PostID, CategoryID, qua đó
            // tạo quan hệ many to many giữa Post và Category
            builder.Entity<PostCategory>().HasKey(p => new { p.PostID, p.CategoryID });

        }

    }

}