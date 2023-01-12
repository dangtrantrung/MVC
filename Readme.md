## Controller

- Controller là 1 lớp kế thừa từ  lớp Controller: Microsoft.AspnetCore.Mvc.Controller
- Action trong Controller là 1 public method không được static
- Action trả về bất kỳ kiểu dữ liệu nào, thường là IActionResult
- Các dịch vụ có thể inject vào controller thông qua constructor

## View

- là file.cshtml
- View cho Action Name lưu tại :/View/ControllerName/ActionName.cshtml
- Thêm thư mục lưu trữ View:
  //View/controller/action.cshtml
  // /MyView/Controller/action
  //{0}=>ten  action
  //{1}=>ten  Controller
  //{2}=>ten  Area
  //RazorViewEngine.ViewExtension .cshtml
- options.ViewLocationFormats.Add("/MyViews/{1}/{0}"+RazorViewEngine.ViewExtension);

## Truyền dữ liệu sang View

- Model
- ViewData
- ViewBag -dynamic
- TempData

## Route, Area, Url Generation 
- MapControllerRoute
- MapAreaControllerRoute
- Url.Action
- Url.ActionLink
- Url.RouteUrl

## Html Tag Helper
- asp-area
- asp-route
- asp-action
- asp-controller
