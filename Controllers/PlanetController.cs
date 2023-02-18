using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Service;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{  [Route("he-mat-troi/[action]")]
    public class PlanetController : Controller
    {
        private readonly PlanetService _planetService;
        private readonly ILogger<PlanetController> _logger;
        
     public PlanetController (PlanetService planetservice, ILogger<PlanetController> logger)
     {
         _planetService=planetservice;
         _logger=logger;
     }
        //[Route("danh-sach-cac-hanh-tinh.html")]
        //[Route("index.html")]
        public IActionResult Index()
        {
            return View();
        }
        
        [BindProperty(SupportsGet =true, Name="action")]
        public string Name {get;set;}
        public IActionResult Mercury()
        {
          var planet= _planetService.Where(p=>p.Name==Name).FirstOrDefault();

             return View("Detail",planet);
        }
         public IActionResult Neptune()
        {
          var planet= _planetService.Where(p=>p.Name==Name).FirstOrDefault();

             return View("Detail",planet);
        }
         public IActionResult Saturn()
        {
          var planet= _planetService.Where(p=>p.Name==Name).FirstOrDefault();

             return View("Detail",planet);
        } public IActionResult Venus()
        {
          var planet= _planetService.Where(p=>p.Name==Name).FirstOrDefault();

             return View("Detail",planet);
        }
         public IActionResult Uranus()
        {
          var planet= _planetService.Where(p=>p.Name==Name).FirstOrDefault();

             return View("Detail",planet);
        }
        [HttpGet("/saomoc.html")] //thiết lập địa chỉ url truy cập + phương thức GET
         public IActionResult Jupiter()
        {
          var planet= _planetService.Where(p=>p.Name==Name).FirstOrDefault();

             return View("Detail",planet);
        }
        //[Route("sao/[action]/{id:int}", Order=2,Name ="Mars")]
         public IActionResult Mars()
        {
          var planet= _planetService.Where(p=>p.Name==Name).FirstOrDefault();

             return View("Detail",planet);
        }
         public IActionResult Earth()
        {
          var planet= _planetService.Where(p=>p.Name==Name).FirstOrDefault();

             return View("Detail",planet);
        }
   [Route("hanh-tinh/{id:int}", Order=1,Name ="planetinfo1")]
   [Route("sao/[action]/{id:int}", Order=2,Name ="planetinfo2")]
   [Route("[controller]-[action].html/{id:int}", Order=3,Name ="planetinfo3")]//do uu tien de phat s inh url
        public IActionResult PlanetInfo(int id)
        {

            var planet= _planetService.Where(p=>p.Id==id).FirstOrDefault();
            return View("detail", planet);
        }
    }
}