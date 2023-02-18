using App.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace App.Components
{
[ViewComponent]
public class CategoryProductSideBar:ViewComponent
{
    public class CategoryProductSideBarData
    {
         public List<CategoryProduct> Categories {get;set;}
         public int level {get;set;}
         public string categoryslug {get;set;}

    }
     public IViewComponentResult Invoke(CategoryProductSideBarData data)
     {
         return View(data);
     }
}


}