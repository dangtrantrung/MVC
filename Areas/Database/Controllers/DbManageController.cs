using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/{action=Index}")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public DbManageController (AppDbContext appDbContext)
        {
            _appDbContext=appDbContext;
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
    }


}