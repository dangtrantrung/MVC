using App.Models.Contacts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using App.Models.Blog;

namespace App.Models
{
    public class AppDbContext:IdentityDbContext<AppUser>
    {
       
      //private const string connectstring ="Data Source=DTTRUNG-PC\\SQLEXPRESS;Initial Catalog=RAZORWEBDB;User ID=sa;Password=tr";
      public AppDbContext (DbContextOptions<AppDbContext> options):base(options)
      {
          // option se dc DI inject
        
      }

     
      protected override void OnConfiguring (DbContextOptionsBuilder builder)
      {
          base.OnConfiguring(builder);
         // builder.UseSqlServer(connectstring);

      }

      protected override void OnModelCreating (ModelBuilder modelbuilder)
      {
        base.OnModelCreating(modelbuilder);
        foreach (var entitytype in modelbuilder.Model.GetEntityTypes())
        {
           var tablename=entitytype.GetTableName();
           if(tablename.StartsWith("AspNet"))
           {
             entitytype.SetTableName(tablename.Substring(6));
           }
        }
        //Fluent API
        modelbuilder.Entity<App.Models.Blog.Category>(entity=>
        {
                 entity.HasIndex(c=>c.Slug)
                       .IsUnique();
        });

        // Chỉ có thể dùng Fluent API để tạo trường khóa chính, duy nhất từ 2 propeties Key của table PostCategory
        // không dùng Attribute key dc[Key]
        modelbuilder.Entity<PostCategory>(entity=>
        {
               entity.HasKey( c=>new {c.PostID, c.CategoryID});
        });

        modelbuilder.Entity<Post>(entity=>
        {
          entity.HasIndex(p=>p.Slug)
                 .IsUnique(); // index de tìm kiem cho nhanh, unique

        });
      }
      // public DbSet<Article> articles {get;set;}
         public DbSet<Contact> Contacts {get;set;}
          public DbSet<App.Models.Blog.Category> Categories{get;set;}
         public DbSet<App.Models.Blog.Post> Posts{get;set;}

          public DbSet<App.Models.Blog.PostCategory> PostCategories{get;set;}

    }
}