using System.Net;
using App.ExtendMethods;
using App.Models;
using App.Service;
using App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.FileProviders;
using App.Menu;
using Microsoft.AspNetCore.Mvc.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options=>{

string connectstring=builder.Configuration.GetConnectionString("AppMvcConnectString");
options.UseSqlServer(connectstring);
});
/* builder.WebHost.ConfigureKestrel(kestrelServerOptions =>
{
    // ...
    // Thiết lập lắng nghe trên cổng 8090 với IP bất kỳ
    kestrelServerOptions.Listen(IPAddress.Any, 8090);
}); */

builder.WebHost.UseKestrel().UseUrls("http://0.0.0.0:8090", "https://0.0.0.0:8091");
// Add services to the container.
builder.Services.AddControllersWithViews(); // dang ky các services theo MVC va cả Razor Pages
builder.Services.AddRazorPages();
builder.Services.Configure<RazorViewEngineOptions>(options=>
{
   //View/controller/action.cshtml
   // /MyView/Controller/action
   //{0}=>ten  action
   //{1}=>ten  Controller
   //{2}=>ten  Area
  //RazorViewEngine.ViewExtension .cshtml
   options.ViewLocationFormats.Add("/MyViews/{1}/{0}"+RazorViewEngine.ViewExtension);
});

// Add transient tu dong logger luu dấu vết h{oạt đông của app
//builder.Services.AddTransient(typeof(ILogger<>), typeof(Logger<>));//Add transient tu dong logger
// // tạo 1 dịch vụ cho mỗi truy vấn  -Request trong từng phiên session, AddTransient
// AddScope ( tạo mới 1 dịch vụ cho 1 phiên  - session)
builder.Services.AddSingleton<ProductService>(); // tạo 1 dịch vụ cho suốt app, khởi tao đối tượng
builder.Services.AddSingleton<ProductService,ProductService>(); // tạo 1 dịch vụ cho suốt app, khởi tạo đối tượng và các đối tượng kế thừa cùng kiểu này
builder.Services.AddSingleton<PlanetService>();

//Add MailService
builder.Services.AddOptions();
var mailsetting=builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailsetting);
builder.Services.AddSingleton<IEmailSender,SendMailService>();

// Add IdentityErrorDescriber
builder.Services.AddSingleton<IdentityErrorDescriber,AppIdentityErrorDescriber>();


 /* builder.Services.AddIdentity<AppUser,IdentityRole> ()
                .AddEntityFrameworkStores<MyBlogContext>()
                .AddDefaultTokenProviders();  */

            
var identityservice = builder.Services.AddIdentity<AppUser, IdentityRole>();

// Thêm triển khai EF lưu trữ thông tin về Idetity (theo AppDbContext -> MS SQL Server).
identityservice.AddEntityFrameworkStores<AppDbContext>();

// Thêm Token Provider - nó sử dụng để phát sinh token (reset password, confirm email ...)
// đổi email, số điện thoại ...
identityservice.AddDefaultTokenProviders(); 

// Truy cập IdentityOptions
builder.Services.Configure<IdentityOptions> (options => {
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes (1); // Khóa 1 phút
    options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 5 lần thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất

   // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount=true;
    
});

//Add Google, Facebook Authentication
/* builder.Services.AddAuthentication()
                .AddGoogle(options=>{
                  var gconfig=builder.Configuration.GetSection("Authentication:Google");
                  options.ClientId=gconfig["ClientId"];
                  options.ClientSecret=gconfig["ClientSecret"];
                  options.CallbackPath="/dang-nhap-tu-google";
                })
                .AddFacebook(options=>{
                  var Fconfig=builder.Configuration.GetSection("Authentication:Facebook");
                  options.ClientId=Fconfig["AppId"];
                  options.ClientSecret=Fconfig["AppSecret"];
                  options.CallbackPath="/dang-nhap-tu-Facebook";
                })
; */
//cấu hình service=> authorize, login, logout, accessdenied

// Cấu hình Cookie
builder.Services.ConfigureApplicationCookie (options => {
    // options.Cookie.HttpOnly = true;  
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  
    options.LoginPath = $"/login/";                                 // Url đến trang đăng nhập
    options.LogoutPath = $"/logout/";   
    options.AccessDeniedPath = $"/AccessDenied/";   // Trang khi User bị cấm truy cập
});

//Dang ky DI dich vụ Appservice AppIdentityErrorDescriber
builder.Services.AddSingleton<IdentityErrorDescriber,AppIdentityErrorDescriber>();

// Trên 30 giây truy cập lại sẽ nạp lại thông tin User (Role)
// SecurityStamp trong bảng User đổi -> nạp lại thông tin Security
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromSeconds(30);
});


//add policy manage
builder.Services.AddAuthorization(options=>
{
 options.AddPolicy("ViewManageMenu",builder=>
 {
      builder.RequireAuthenticatedUser();
      builder.RequireRole(App.Data.RoleName.Administrator);
 });
});

builder.Services.AddDistributedMemoryCache();           // Đăng ký dịch vụ lưu cache trong bộ nhớ (Session sẽ sử dụng nó)
builder.Services.AddSession(cfg => {                    // Đăng ký dịch vụ Session
    cfg.Cookie.Name = "xuanthulab";             // Đặt tên Session - tên này sử dụng ở Browser (Cookie)
    cfg.IdleTimeout = new TimeSpan(0,30, 0);    // Thời gian tồn tại của Session
});

builder.Services.AddTransient<CartService>();
builder.Services.AddTransient<IActionContextAccessor,ActionContextAccessor>();
builder.Services.AddTransient<AdminSideBarService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// khi truy cập address "/contents/1.jpg" => mở file "/Uploads/1.jpg"
app.UseStaticFiles(new StaticFileOptions()
{
        FileProvider=new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(),"Uploads")
        ),
        RequestPath="/contents"
});
app.UseSession();
app.UseRouting(); //endpoint routing middlewareapp.MapRazorPages()
app.UseAuthentication();// xac dinh danh tinh Identity
app.UseAuthorization();

app.AddStatusCodePage(); // đã extend method sử dụng middleware app.UseStatuscodepage();
 
//xử lý URL theo cấu trúc pattern /{controller}/{action}/{id?}
/* app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); */

//app.MapRazorPages(); //  app MVC này co the truy cap den các trang Razor


app.UseEndpoints(endpoints=>
{
       endpoints.MapGet("/sayhi",async (context)=>
       {
           await context.Response.WriteAsync($"Hello ASP.NET MVC {DateTime.Now}");
       });
       endpoints.MapRazorPages();

        // action, controller,area
       endpoints.MapControllerRoute(
           name:"first",
           //pattern:"start-here/{controller=Home}/{action=Index}/{id?}"/* ,
           //pattern:"xemsanpham/{id?}",
           pattern:"{url:regex(^((xemsanpham)|(viewproduct))$)}/{id:range(2,4)}",
           defaults: new {
               controller="First",
               action="ViewProduct",
               //id=1
           },
           constraints: new {
               /* url= new StringRouteConstraint("xemsanpham"), */
               //RegexRouteConstraint
               //url =new RegexRouteConstraint(@"^((xemsanpham)|(viewproduct))$"),
               //id= new RangeRouteConstraint(3,4)

           }
           
          /*   defaults: new {
               controller="First",
               action="ViewProduct",
               id=3
           }  */
       
        );
          // action, controller,area
       endpoints.MapControllerRoute(
           name:"default",
           //pattern:"start-here/{controller=Home}/{action=Index}/{id?}"/* ,
           
          /*   defaults: new {
               controller="First",
               action="ViewProduct",
               id=3
           }  */
         pattern:"/{controller=Home}/{action=Index}/{id?}"  // chỉ ánh xạ những controller không có Area Attribute
        );

        endpoints.MapAreaControllerRoute(
           name:"product",
           pattern:"/{controller}/{action=Index}/{id?}" ,
           areaName: "ProductManage"
        );
}
);




app.Run();

//tạo controllers
//dotnet aspnet-codegenerator controller -name PostController -namespace App.Areas.Blog.Controllers
 //-outDir Areas/Blog/Controllers/ -m App.Models.Blog.Post -dc App.Models.AppDbContext -udl
