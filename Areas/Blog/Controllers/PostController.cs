using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Blog;
using App.Data;
using Microsoft.AspNetCore.Authorization;
using App.Areas.Blog.Models;
using Microsoft.AspNetCore.Identity;
using App.Utilities;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/post/[action]/{id?}")]
    [Authorize(Roles=RoleName.Administrator +","+RoleName.Editor)]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PostController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
          
        }



        // GET: Post
        public async Task<IActionResult> Index([FromQuery(Name="p")]int currentPage, int pagesize)
        {
            var posts=_context.Posts
                    .Include(p=>p.Author)
                    .OrderByDescending(p=>p.DateUpdated);
                    
            if(pagesize<=0) pagesize=10;
            int totalPosts= await posts.CountAsync();
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

            ViewBag.postIndex=(currentPage -1) *pagesize;
            ViewBag.pagingModel =pagingmodel;
            ViewBag.totalPosts=totalPosts;
            
            
            //do sang View so bai post cua trang truy van hien tai
            var postinPage = await posts.Skip((currentPage-1)*pagesize)
                                    .Take(pagesize)
                                    .Include(p=>p.PostCategories)
                                    .ThenInclude(pc=>pc.Category)
                                    .ToListAsync();
            return View(postinPage);
        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
           // ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "UserName");

           var categories= await _context.Categories.ToListAsync();
           ViewData["Categories"]=new MultiSelectList(categories,"Id", "Title");
           return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIDs")] CreatePostModel post)
        {
            
            if (post.Slug==null)
                {
                  post.Slug = AppUtilities.GenerateSlug(post.Title);
                }

            if (await  _context.Posts.AnyAsync(p=>p.Slug==post.Slug))
            {
                  ModelState.AddModelError("Slug trùng nhau","Nhập chuỗi Url khác -- trong CSDL đã có slug - Url này rồi");
                  return View(post);
            }
            if (ModelState.IsValid)
            {
                var user= await _userManager.GetUserAsync(this.User);

                post.DateCreated=post.DateUpdated=DateTime.Now;
                post.AuthorId=user.Id;
                
                _context.Add(post);

                // Thêm CategoryID vào bảng PostCategory trong CSDL
                if( post.CategoryIDs!=null)
                {
                    foreach (var catID in post.CategoryIDs)
                    {
                        _context.Add( new PostCategory()
                        {
                            
                            CategoryID=catID,
                            Post=post
                        });
                    }
                }
                await _context.SaveChangesAsync();
                StatusMessage="Bạn vừa tạo bài viết mới: "+ post.Title;
                return RedirectToAction(nameof(Index));
            }
            //ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "UserName", post.AuthorId);
            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }
            //var post=await _context.Posts.FindAsync(id); find async có thể k lấy ra dc pc
            var post= await _context.Posts.Include(p=>p.PostCategories).FirstOrDefaultAsync(p=>p.PostId==id);
            if(post==null)
            {
                 return NotFound();
            }

            var postEdit = new CreatePostModel()
            {
                PostId=post.PostId,
                Title= post.Title,
                Content=post.Content,
                Description=post.Description,
                Slug=post.Slug,
                Published=post.Published,
                CategoryIDs=post.PostCategories.Select(pc=>pc.CategoryID).ToArray()

            };
            
           var categories= await _context.Categories.ToListAsync();
            ViewData["Categories"]=new MultiSelectList(categories,"Id", "Title");
            return View(postEdit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published,CategoryIDs")] CreatePostModel post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }
            var categories= await _context.Categories.ToListAsync();
            ViewData["Categories"]=new MultiSelectList(categories,"Id", "Title");
            
            if (post.Slug==null)
                {
                  post.Slug = AppUtilities.GenerateSlug(post.Title);
                }

            if (await  _context.Posts.AnyAsync(p=>p.Slug==post.Slug&&p.PostId!=id))
            {
                  ModelState.AddModelError("Slug trùng nhau","Nhập chuỗi Url khác -- trong CSDL đã có slug - Url này rồi");
                  return View(post);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate= await _context.Posts.Include(p=>p.PostCategories).FirstOrDefaultAsync(p=>p.PostId==id);
                   if(postUpdate==null)
                   {
                       return NotFound();
                   }
                      postUpdate.Title=post.Title;
                      postUpdate.Description=post.Description;
                      postUpdate.Content=post.Content;
                      postUpdate.Slug=post.Slug;
                      postUpdate.Published=post.Published;
                      postUpdate.DateUpdated=DateTime.Now;
                      //Update Post Category
                      if(post.CategoryIDs==null)post.CategoryIDs= new int[]{};
                      var oldCateIds= postUpdate.PostCategories.Select(c=>c.CategoryID).ToArray();
                      var newCateIds= post.CategoryIDs;
                      var removecatIds= from postcat in postUpdate.PostCategories
                                    where (!newCateIds.Contains(postcat.CategoryID))
                                    select postcat;
                     _context.PostCategories.RemoveRange(removecatIds);

                     var addcatIds= from CatId in newCateIds
                                     where !oldCateIds.Contains(CatId)
                                     select CatId;
                   foreach (var CatId in addcatIds)
                   {

                        _context.PostCategories.Add(new PostCategory(){
                            PostID=id,
                            CategoryID=CatId
                        });
                   }          

                    _context.Update(postUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage="Bạn vừa cập nhật bài viết: " + post.Title;
                return RedirectToAction(nameof(Index));
            }
           // ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(post);
        }
         [TempData]
         public string StatusMessage {get; set;}
        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'AppDbContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            StatusMessage ="Bạn vừa xóa bài viết " +post.Title;
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
