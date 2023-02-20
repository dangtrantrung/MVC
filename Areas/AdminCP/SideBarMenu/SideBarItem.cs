using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace App.Menu

{  
    // OOP- phan loai: cac loai khac nhau trong class
   public enum SideBarItemType
   {
       Divider,
       Heading,
       NavItem

   }
    public class SideBarItem
    {
         //phan tich cac dac diem, thuoc tinh, thanh phan cua class, item trong class
          public string Title {get;set;}
          public bool IsActive {get;set;}
          public SideBarItemType Type {get;set;}

          public string Controller {get;set;}
          public string Action {get;set;}
          public string Area {get;set;}

          public string AwesomeIcon {get;set;} //fas fa-fw fa-cog

          //cac quan he 1-1, 1-n, cha- con trong class, ngoai class
          //item nay la don gian hay phuc tap --- co components - co cac item con ben trong?
          public List <SideBarItem> Items {get;set;}

          public string collapseID {get;set;}

          // cac hanh vi cua cac item torng class dua tren cac thuoc tinh
          public string GetLink (IUrlHelper urlHelper)
          {
              return urlHelper.Action(Action,Controller, new {area=Area});
          }

          public string RenderHtml(IUrlHelper urlHelper)
          {
              var html= new StringBuilder();

              if(Type==SideBarItemType.Divider)
              {
                 html.Append("<hr class=\"sidebar-divider my-1\">");
              }
              else if(Type==SideBarItemType.Heading)
              {    //@$ viet chuoi tren nhieu dong, loai ky tu " dung ""
                  html.Append(@$"<div class=""sidebar-heading"">
                                  {Title}
                               </div>");
                               
              }
              else if(Type==SideBarItemType.NavItem)
              {
                  if(Items==null)
                  {  var url = GetLink(urlHelper);
                      var icon = (AwesomeIcon!=null)? $"<i class=\"{AwesomeIcon}\"></i>":"";
                      var cssClass="nav-item";
                      if(IsActive) cssClass+=" active";

                       html.Append (@$"<li class=""{cssClass}"">
                                        <a class=""nav-link"" href=""{url}"">
                                         {icon}
                                         <span>{Title}</span></a>
                                        </li>");
                  }
                  else
                  {
                     var url = GetLink(urlHelper);
                      var icon = (AwesomeIcon!=null)? $"<i class=\"{AwesomeIcon}\"></i>":"";
                      var cssClass="nav-item";
                      var collapseCss="collapse";
                      if(IsActive) 
                      {
                          cssClass+=" active";
                             collapseCss+=" show";
                      }
                      var itemMenu ="";
                      foreach (var item in Items)
                      {
                          var urlItem=item.GetLink(urlHelper);
                          var cssItem="collapse-item";
                          var iconItem = (item.AwesomeIcon!=null)? $"<i class=\"{item.AwesomeIcon}\"></i>":"";
                          if(item.IsActive) cssItem+=" active";
                          itemMenu+=$"<a class=\"{cssItem}\" href=\"{urlItem}\">{item.Title}</a>";
                      }

                     html.Append(@$"<li class=""{cssClass}"">
                <a class=""nav-link collapsed"" href=""#"" data-toggle=""collapse"" data-target=""#{collapseID}""
                    aria-expanded=""true"" aria-controls=""{collapseID}"">
                    {icon}
                    <span>{Title}</span>
                </a>
                <div id=""{collapseID}"" class=""{collapseCss}"" aria-labelledby=""headingTwo"" data-parent=""#accordionSidebar"">
                    <div class=""bg-white py-2 collapse-inner rounded"">
                        <h6 class=""collapse-header"">Custom Components:</h6>
                        {itemMenu}
                    </div>
                </div>
            </li> ");

                  }

              }

              return html.ToString();
          }
    }
}