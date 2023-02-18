using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {

        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;

        public ViewPostController(ILogger<ViewPostController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        /* "/post/"
        "/post/{categoryslug?}" */
        [Route("/post/{categoryslug?}")]
        public IActionResult Index(string categoryslug, [FromQuery(Name="p")]int currentPage, int pagesize)
        {
            //return Content(categoryslug);
            var categories =GetCategories();
            ViewBag.categories=categories;
            ViewBag.categoryslug=categoryslug;


            Category category =null;
            if(!string.IsNullOrEmpty(categoryslug))
            {
                category=_context.Categories.Where(c=>c.Slug==categoryslug)
                               .Include(c=>c.CategoryChildren)
                               .FirstOrDefault();
                if(category==null)
                {
                    return NotFound("Không thấy category này trong CSDL");
                }
                ViewBag.category=category;
            }
            
            
            var posts =_context.Posts
                              .Include(p=>p.Author)
                              .Include(p=>p.PostCategories)
                              .ThenInclude(p=>p.Category)
                              .AsQueryable();
           posts=posts.OrderByDescending(p=>p.DateUpdated);
          if(category!=null)
            {
               var ids= new List<int>();
                category.ChildrenCategoryIDS(ids,null);
                ids.Add(category.Id);
                posts=posts.Where(p=>p.PostCategories.Where(pc=>ids.Contains(pc.CategoryID)).Any());
            }           
                           

            int totalPosts= posts.Count();
            if(pagesize<=0) pagesize=10;
          //  int numberofPostsPerPage =10;
            int countPages= (int)Math.Ceiling((double)totalPosts/pagesize);
            
            if(currentPage>countPages) currentPage =countPages;
            if(currentPage<1) currentPage=1;
            
            var pagingmodel =new PagingModel()
            {
                countpages=countPages,
                currentpage=currentPage,
                generateUrl=(pagenumber)=> Url.Action("Index", new {
                     p=pagenumber,
                     pagesize=pagesize
                    })                         
                
            };

            //do sang View so bai post cua trang truy van hien tai
            var postinPage = posts.Skip((currentPage-1)*pagesize)
                                    .Take(pagesize);
                                    

            //ViewBag.postIndex=(currentPage -1) *pagesize;
            ViewBag.pagingModel =pagingmodel;
            ViewBag.totalPosts=totalPosts;   
            ViewBag.category=category;         

                                
           return View(postinPage.ToList());
        }
        [Route("/post/{postslug}.html")]
        public IActionResult Detail(string postslug)
        {

             //return Content(categoryslug);
            var categories =GetCategories();
            ViewBag.categories=categories;
            var post =_context.Posts.Where(p=>p.Slug==postslug)
                                    .Include(p=>p.Author)
                                    .Include(p=>p.PostCategories)
                                    .ThenInclude(pc=>pc.Category).FirstOrDefault();
            if(post==null)
            {
                return NotFound("Không thấy bài viết");
            }
            Category category =post.PostCategories.FirstOrDefault()?.Category;
            ViewBag.category=category;

            //lấy ra 5 bài viết gần nhất
            var otherPosts= _context.Posts.Where(p=>p.PostCategories.Any(c=>c.Category.Id==category.Id))
                                          .Where(p=>p.PostId!=post.PostId)
                                          .OrderByDescending(p=>p.DateUpdated)
                                          .Take(5);
            ViewBag.otherPosts=otherPosts;
            
            return View(post);

        }

        private List<Category> GetCategories()
        {
            var categories=_context.Categories
                           .Include(c=>c.CategoryChildren)
                           .AsEnumerable()
                           .Where(c=>c.ParentCategory==null)
                           .ToList();
            return categories;
        }


    }
    //git commit -m"View Post with Breadcrum Media Object Bootstrap 5"
}