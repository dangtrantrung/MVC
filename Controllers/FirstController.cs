using App.Service;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{

public class FirstController:Controller
{
    private readonly ILogger<FirstController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly ProductService _productService;

        public FirstController(ILogger<FirstController> logger, IWebHostEnvironment env, ProductService productService)
        {
            _logger = logger;
            _env = env;
            _productService = productService;
        }

        public string Index() 
    {
        _logger.Log(LogLevel.Warning,"THông báo ABC");
        _logger.LogWarning("THông báo ABC");
         _logger.LogDebug("THông báo ABC");
          _logger.LogCritical("THông báo ABC");
        _logger.LogInformation("Đang vào Action: Index Action");
        //Serilog=> log luu tren sql, tuy bien log, 

        //Console.WriteLine("Đang vào Action: Index Action");
                  

        var httpcontext= this.HttpContext;
        var request=this.Request;
        /* this.Response; */
        //this.User
        //this.ModelStates
        //this.ViewData
        //this.ViewBag
        //this.Url
        //this.TempData


          return $"Tôi là Index của First với User là {this.User} và Url là: {this.Url}";
    } 

    //Các Action

    public void Nothing()
    {
        _logger.LogInformation("Nothing Action");
        Response.Headers.Add("hi","Xin chao cac ban");
    }
    public object Anything()=>new int[]{1,2,3};

    //IActionResult
    /* Kiểu trả về                 | Phương thức
    ------------------------------------------------
    ContentResult               | Content()
    EmptyResult                 | new EmptyResult()
    FileResult                  | File()
    ForbidResult                | Forbid()
    JsonResult                  | Json()
    LocalRedirectResult         | LocalRedirect()
    RedirectResult              | Redirect()
    RedirectToActionResult      | RedirectToAction()
    RedirectToPageResult        | RedirectToRoute()
    RedirectToRouteResult       | RedirectToPage()
    PartialViewResult           | PartialView()
    ViewComponentResult         | ViewComponent()
    StatusCodeResult            | StatusCode()
    ViewResult                  | View() */
   public IActionResult Readme()
   {
       var content=@"
       Xin chào cac ban,
         các ban đang học ASP.NET MVC


         Xuanthu LAB


       ";
       string contentRootPath = _env.ContentRootPath;
            string webRootPath = _env.WebRootPath;

            
        
       return Content(content+"<br>"+"ProjectPath:"+"<br>" +contentRootPath + "<br>"+"WebrootPath:"+"<br>"+ webRootPath,"text/html");
   }
   public IActionResult Bird()
   {  
       string filePath=Path.Combine(_env.ContentRootPath,"Files","birds.jpg");
       var bytes=System.IO.File.ReadAllBytes(filePath);
       return File(bytes,"image/jpg");

   }
   public IActionResult IPhonePrice()
   {
       return Json(
       new {
           productName="Iphone",
           price=1000
       }
       );
   }

   public IActionResult Privacy()
   {
       var url =Url.Action("Privacy","Home");
        _logger.LogInformation("chuyển hướng đến: "+url);
      return LocalRedirect(url); //url la local address
   }
    public IActionResult Google()
   {
       var url ="https://google.com";
        _logger.LogInformation("chuyển hướng đến: "+url);
      return Redirect(url); //url la local address
   }

   public IActionResult HelloView(string username)
   {
   if(string.IsNullOrEmpty(username))
   {
       username="Khách";
   }
   
       //View(template) => yeu cầu razor engine đọc file .cshtml
       //View(template, model)
       //View(template) //xinchao2.cshtml=>View/First
      // return View("xinchao2",username);
       //return View("/MyViews/xinchao1.cshtml",username);
      // HelloView.cshtml trong=>View/First
      //return View((object)username);
      return View("xinchao3",username);
   }
   [TempData]
   public string StatusMessage{get;set;}
   //tham so id co the binding tu Url pattern trong Programe.cs
  
  
   //[AcceptVerbs]
//[Route]
/* [HttpGet]
[HttpPost]
[HttpHead]
[HttpDelete]
[HttpPatch]
[HttpPut] */

[AcceptVerbs("POST","GET")]
   public IActionResult ViewProduct(int? id)
   {
       var product = 
       _productService.Where(p=>p.Id==id).FirstOrDefault();

       if(product==null) 
       {  
           StatusMessage="Đang chuyển hướng -do sản phẩm bạn yêu cầu không có";
           return Redirect(Url.Action("Index","Home"));
       }
       //Views/First/ViewProduct.cshtml
      //return View(product);

      //View Data
     /* this.ViewData["product"]=product;
     ViewData["Title"]=product.Name;
     return View("ViewProduct2"); */

     //ViewBag dynamic set thuộc tinh tại thời điển runtime
     ViewBag.product=product;
     return View("ViewProduct3");

     //TempData truyền dữ liệu giữa các Url các views trong 1 Session
     
   }

   
   
}

}
