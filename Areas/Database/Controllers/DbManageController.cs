using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                  var useradmin= await _userManager.FindByEmailAsync("admin");

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
                  }

                   StatusMessage="Vừa seed Database";
                   return RedirectToAction("Index");
        }
    }


}