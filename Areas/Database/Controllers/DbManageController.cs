using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bogus;
using App.Models.Blog;

namespace App.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/{action=Index}")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _appDbContext;
         private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbManageController(AppDbContext appDbContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            
        }

        public IActionResult Index()
        {
            return View();
        }
     [HttpGet]
        public IActionResult DeleteDb()
        {
            return View();
        }
        [TempData]
        public string StatusMessage {get;set;}

        [HttpPost]
        public async Task<IActionResult> DeleteDbAsync()
        {
            var success=await _appDbContext.Database.EnsureDeletedAsync();
            StatusMessage= success?"Xóa thành công":"Xóa DB Không thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MigrateDb()
        {
            await _appDbContext.Database.MigrateAsync();
            StatusMessage= "Cập nhật DB thành công";
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> SeedDataAsync()
        {   
                //Create 3 seed Roles
                 var rolenames= typeof(Data.RoleName).GetFields().ToList(); // reflection for 1 class
                  foreach (var r in rolenames)
                  {
                      var rolename=(string)r.GetRawConstantValue();
                      var rfound=await _roleManager.FindByNameAsync(rolename);

                      if(rfound==null)
                      {
                          await _roleManager.CreateAsync(new IdentityRole(rolename));

                      }
                  }
                  //create user admin with role administrator
                  var useradmin= await _userManager.FindByNameAsync("admin");

                  if(useradmin==null)
                  {                      
                         useradmin= new AppUser(   )
                      {
                          UserName="admin",
                          Email="admin@example.com",
                          EmailConfirmed=true
                      };
                      await _userManager.CreateAsync(useradmin,"admin123");
                      await _userManager.AddToRoleAsync(useradmin,Data.RoleName.Administrator);
                      StatusMessage="Vừa seed User Database";
                     
                  }
                
                SeedPostCategory();

                StatusMessage="Vừa seed Bogus - PostCategpry Database";
                return RedirectToAction("Index");
        }
        private void SeedPostCategory()
        {

            // Xóa các fake dta trước khi add
            _appDbContext.RemoveRange(_appDbContext.Categories.Where(c=>c.Content.Contains("[fakeData]")));
             _appDbContext.RemoveRange(_appDbContext.Posts.Where(p=>p.Content.Contains("[fakeData]")));
            var fakerCategory =new Faker<Category>();
            int cm= 1;
            fakerCategory.RuleFor(c=>c.Title, fk=>$"CM{cm++} "+fk.Lorem.Sentence(1,2).Trim('.'));
            fakerCategory.RuleFor(c=>c.Content, fk=>fk.Lorem.Sentences(5) +"[fakeData]");
            fakerCategory.RuleFor(c=>c.Slug, fk=>fk.Lorem.Slug());

            var cate1=fakerCategory.Generate();
                     var cate11=fakerCategory.Generate();
                     var cate12=fakerCategory.Generate();
            var cate2=fakerCategory.Generate();
                    var cate21=fakerCategory.Generate();
                    var cate211=fakerCategory.Generate();
            cate11.ParentCategory=cate1;
            cate12.ParentCategory=cate1;
            cate21.ParentCategory=cate2;
            cate211.ParentCategory=cate21;
            var categories= new Category[]{cate1,cate2,cate12,cate11,cate21,cate211,};
            _appDbContext.Categories.AddRange(categories);
            _appDbContext.SaveChanges();

            //Tạo POST
            var rCateIndex=new Random();
            int bv =1;
            var user= _userManager.GetUserAsync(this.User).Result;
            var fakerPost= new Faker<Post>();

            fakerPost.RuleFor(p=>p.AuthorId, f=>user.Id);
           
            fakerPost.RuleFor(p=>p.Content,f=>f.Lorem.Paragraphs(7)+"[fakeData]");
            fakerPost.RuleFor(p=>p.DateCreated,f=>f.Date.Between( new DateTime(2022,1,1), new DateTime(2023,1,1)));
            fakerPost.RuleFor(p=>p.Description,f=>f.Lorem.Sentences(3));
             fakerPost.RuleFor(p=>p.Published,f=>true);
              fakerPost.RuleFor(p=>p.Slug,f=>f.Lorem.Slug());
               fakerPost.RuleFor(p=>p.Title,f=>$"Bài {bv++} "+f.Lorem.Sentence(3,4).Trim('.'));

               List<Post> posts= new List<Post>();
              List<PostCategory> post_category= new List<PostCategory>();
             for(int i=0;i<40;i++)
             {
                 var post= fakerPost.Generate();
                 post.DateUpdated=post.DateCreated;
                //post.Author=(AppUser)user.UserName;
                 posts.Add(post);
                 post_category.Add(new PostCategory()
                     {
                         Post=post,
                         Category=categories[rCateIndex.Next(5)]
                     });

             }
             _appDbContext.AddRange(posts);
             _appDbContext.AddRange(post_category);
              _appDbContext.SaveChanges();


        }
    }


}