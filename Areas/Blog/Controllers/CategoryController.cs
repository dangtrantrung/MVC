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

namespace App.Areas.Blog.Controllers
{
   [Area("Blog")]
   [Route("admin/blog/category/[action]/{id?}")]
   [Authorize(Roles=RoleName.Administrator)]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            //var appDbContext = _context.Categories.Include(c => c.ParentCategory);
            // sửa index
            var qr= (from c in _context.Categories select c)
                     .Include(c=>c.ParentCategory)
                     .Include(c=>c.CategoryChildren);

            var categories= (await qr.ToListAsync())
                            .Where(c=>c.ParentCategory==null) // chỉ lấy danh mục cha
                            .ToList();


            
            //return View(await appDbContext.ToListAsync());
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        private void CreateSelectItems(List<Category> source, List<Category> des, int level)
        {
            string prefix=string.Concat(Enumerable.Repeat("----",level));
              foreach (var category in source)
              {
                  //category.Title= prefix + " "+ category.Title; EF đang giám sát change entity-> khi savechangeAsync -> EF sẽ tự đông đổi tên category trong SQL Server
                  // do vậy phải add new category vào des list<category>
                  des.Add(new Category {
                      Id=category.Id,
                      Title=prefix + " "+ category.Title
                  });
                  if(category.CategoryChildren?.Count>0)
                  {
                       //gọi đệ quy phương thức
                      CreateSelectItems(category.CategoryChildren.ToList(),des,level+1);
                  }
              }
        }

        // GET: Category/Create
       // GET: Blog/Category/Create
public async Task<IActionResult> Create()
{

    var qr= (from c in _context.Categories select c)
                     .Include(c=>c.ParentCategory)
                     .Include(c=>c.CategoryChildren);

    var categories= (await qr.ToListAsync())
                    .Where(c=>c.ParentCategory==null) // chỉ lấy danh mục cha
                    .ToList();
    categories.Insert(0, new Category(){
              Id=-1,
              Title= "Không có danh mục cha"
    });
    var items =new List<Category>();

    CreateSelectItems(categories,items,0);

    var selectlist= new SelectList(items, "Id", "Title");
    // ViewData["ParentId"] = new SelectList(_context.Categories, "Id", "Slug");
    /* var listcategory = await _context.Categories.ToListAsync();
    listcategory.Insert(0, new Category() {
        Title = "Không có danh mục cha",
        Id = -1
    });
    ViewBag.ParentId = new SelectList(listcategory, "Id", "Title", -1);
    return View(); */
     ViewData["ParentCategoryId"] =selectlist;
     return View();

}


[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("Id,ParentCategoryId,Title,Content,Slug")] Category category)
{    
        if(ModelState.IsValid)
        {
            if (category.ParentCategoryId.Value == -1) category.ParentCategoryId = null;
            _context.Add(category);
            await _context.SaveChangesAsync();      
                     
            return RedirectToAction(nameof(Index));
        }
        
      var qr= (from c in _context.Categories select c)
                     .Include(c=>c.ParentCategory)
                     .Include(c=>c.CategoryChildren);

    var categories= (await qr.ToListAsync())
                    .Where(c=>c.ParentCategory==null) // chỉ lấy danh mục cha
                    .ToList();
    categories.Insert(0, new Category(){
              Id=-1,
              Title= "Không có danh mục cha"
    });
    var items =new List<Category>();

    CreateSelectItems(categories,items,0);
    
    var selectlist= new SelectList(items, "Id", "Title");

 ViewData["ParentCategoryId"] =selectlist;
 return View (category);
}



        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
             var qr= (from c in _context.Categories select c)
                     .Include(c=>c.ParentCategory)
                     .Include(c=>c.CategoryChildren);

    var categories= (await qr.ToListAsync())
                    .Where(c=>c.ParentCategory==null) // chỉ lấy danh mục cha
                    .ToList();
    categories.Insert(0, new Category(){
              Id=-1,
              Title= "Không có danh mục cha"
    });
    var items =new List<Category>();

    CreateSelectItems(categories,items,0);
    
    var selectlist= new SelectList(items, "Id", "Title");
            ViewData["ParentCategoryId"] = selectlist;
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParentCategoryId,Title,Content,Slug")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }
            bool canUpdate =true;
            if( category.ParentCategoryId==category.Id)
            {
                ModelState.AddModelError(string.Empty,"Lỗi: Phải chọn danh mục cha khác danh mục cần chỉnh sửa");
                canUpdate=false;
            }

            // Kiểm tra thiết lập danh mục cha phù hợp
            if(canUpdate&&category.ParentCategoryId!=null)
            {
               var childCates= (from c in _context.Categories select c)
                               .Include(c=>c.CategoryChildren)
                               .ToList()
                               .Where(c=>c.ParentCategoryId==category.Id);
                // Func check ID
                Func<List<Category>, bool> checkCateIds = null;
                   checkCateIds =(cates) =>
                   {
                          foreach (var cate in cates)
                          {
                              Console.WriteLine(cate.Title);
                              if(cate.Id==category.ParentCategoryId)
                              {
                                  canUpdate=false;
                                  ModelState.AddModelError(string.Empty,"Lỗi: Phải chọn danh mục cha khác vì danh mục cha bạn vừa chọn là danh mục con của danh mục cần chỉnh sửa");
                                  return true;
                              }
                              if(cate.CategoryChildren!=null)                               
                                   return checkCateIds(cate.CategoryChildren.ToList());
                          }
                          
                              return false;
                    };
                    //end Func
                    checkCateIds(childCates.ToList());

            }

            if (ModelState.IsValid&&canUpdate)
            {
                try
                {
                     if (category.ParentCategoryId.Value == -1) category.ParentCategoryId = null;
                   
                   var dtc =_context.Categories.FirstOrDefault(c=>c.Id==id);
                   _context.Entry(dtc).State=EntityState.Detached; // EF bỏ giám sát entity cần edit này
                    
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
           var qr= (from c in _context.Categories select c)
                     .Include(c=>c.ParentCategory)
                     .Include(c=>c.CategoryChildren);

    var categories= (await qr.ToListAsync())
                    .Where(c=>c.ParentCategory==null) // chỉ lấy danh mục cha
                    .ToList();
    categories.Insert(0, new Category(){
              Id=-1,
              Title= "Không có danh mục cha"
    });
    var items =new List<Category>();

    CreateSelectItems(categories,items,0);
    
    var selectlist= new SelectList(items, "Id", "Title");
            ViewData["ParentCategoryId"] = selectlist;
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'AppDbContext.Categories'  is null.");
            }
            var category = await _context.Categories
                            .Include(c=>c.CategoryChildren)
                            .FirstOrDefaultAsync(c=>c.Id==id);
               if (category == null)
            {
               return NotFound();
            }
            foreach (var cCategory in category.CategoryChildren)
            {
                 cCategory.ParentCategoryId=category.ParentCategoryId;
            }

            if (category != null)
            {
                _context.Categories.Remove(category);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
          return _context.Categories.Any(e => e.Id == id);
        }
    }
}
