using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace App.Menu
{

    public class AdminSideBarService
    {
        public List <SideBarItem> Items {get;set;} =new List<SideBarItem>();

        private readonly IUrlHelper UrlHelper;

        public AdminSideBarService( IUrlHelperFactory factory, IActionContextAccessor action)
        {
            
            this.UrlHelper = factory.GetUrlHelper(action.ActionContext);
            //Khoi tao cac muc cua SideBar

            Items.Add(new SideBarItem(){
               Type=SideBarItemType.Divider
            });
            Items.Add(new SideBarItem(){
               Type=SideBarItemType.Heading,
               Title ="MANAGE WEBSITE"
            });
            Items.Add(new SideBarItem(){
               Type=SideBarItemType.NavItem,
               Controller="DbManage",
               Action ="Index",
               Area="Database",
               Title="Manage Database",
               AwesomeIcon="fas fa-database"

            });

            Items.Add(new SideBarItem(){
               Type=SideBarItemType.NavItem,
               Controller="Contact",
               Action ="Index",
               Area="Contact",
               Title="Manage Contact",
               AwesomeIcon="fas fa-address-card"

            });
             Items.Add(new SideBarItem(){
               Type=SideBarItemType.Divider
            });
             Items.Add(new SideBarItem(){
               Type=SideBarItemType.NavItem,
               Title="Manage Members Authorization",
               AwesomeIcon="fas fa-users",
               collapseID="role",
               Items = new List<SideBarItem>(){
                   new SideBarItem(){
                        Type=SideBarItemType.NavItem,
                        Controller="Role",
                        Action ="Index",
                        Area="Identity",
                        Title="Member Role",
                        //IsActive=true,
                       // AwesomeIcon="fa-regular fa-user-group"

            },
                        new SideBarItem(){
                                    Type=SideBarItemType.NavItem,
                                    Controller="Role",
                                    Action ="Create",
                                    Area="Identity",
                                    Title="Create Role",
                                    //IsActive=true,
                                // AwesomeIcon="fa-regular fa-user-group"

                        },
                        new SideBarItem(){
                                    Type=SideBarItemType.NavItem,
                                    Controller="User",
                                    Action ="Index",
                                    Area="Identity",
                                    Title="Member List",
                                    //IsActive=true,
                                // AwesomeIcon="fa-regular fa-user-group"

                        }
               }
                    });
                   Items.Add(new SideBarItem(){Type=SideBarItemType.Divider});
                   Items.Add(new SideBarItem(){
               Type=SideBarItemType.NavItem,
               Title="Manage BLOG",
               AwesomeIcon="fa-brands fa-blogger",
               collapseID="blog",
               Items = new List<SideBarItem>(){
                   new SideBarItem(){
                        Type=SideBarItemType.NavItem,
                        Controller="Category",
                        Action ="Index",
                        Area="Blog",
                        Title="Blog Category",
                        //IsActive=true,
                       // AwesomeIcon="fa-regular fa-user-group"

            },
                        new SideBarItem(){
                                    Type=SideBarItemType.NavItem,
                                    Controller="Category",
                                    Action ="Create",
                                    Area="Blog",
                                    Title="Create Blog Category",
                                    //IsActive=true,
                                // AwesomeIcon="fa-regular fa-user-group"

                        },
                        new SideBarItem(){
                                    Type=SideBarItemType.NavItem,
                                    Controller="Post",
                                    Action ="Index",
                                    Area="Blog",
                                    Title="Post List",
                                    //IsActive=true,
                                // AwesomeIcon="fa-regular fa-user-group"

                        },
                        new SideBarItem(){
                                    Type=SideBarItemType.NavItem,
                                    Controller="Post",
                                    Action ="Create",
                                    Area="Blog",
                                    Title="New Post",
                                    //IsActive=true,
                                // AwesomeIcon="fa-regular fa-user-group"

                        }
               }
                    });

        }

        public string renderHtml ()
        {
            var html = new StringBuilder();

            foreach (var item in Items)
            {
                 html.Append(item.RenderHtml(UrlHelper));
            }
            
            return html.ToString();
        }

        public void SetActive(string Controller, string Action, string Area)
        {
            foreach (var item in Items)
            {
                 if (item.Controller==Controller&&item.Action==Action&&item.Area==Area)
                 {
                     item.IsActive=true;
                     return;
                 }
                 else
                 {
                     if (item.Items!=null)
                     {
                         foreach (var childItem in item.Items)
                         {
                             if (childItem.Controller==Controller&&childItem.Action==Action&&childItem.Area==Area)
                                {
                                    childItem.IsActive=true;
                                    item.IsActive=true;
                                    return;
                                }
                         }
                     }
                 }
            }
            
        }
    }
}