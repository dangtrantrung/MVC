using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Product;
using App.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using App.Utilities;
using App.Areas.Product.Models;
using System.ComponentModel.DataAnnotations;

namespace App.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("admin/productmanage/[action]/{id?}")]
    [Authorize(Roles=RoleName.Administrator +","+RoleName.Editor)]
    public class ProductManageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ProductManageController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
          
        }



        // GET: product
        public async Task<IActionResult> Index([FromQuery(Name="p")]int currentPage, int pagesize)
        {
            var products=_context.ProductS
                    .Include(p=>p.Author)
                    .OrderByDescending(p=>p.DateUpdated);
                    
            if(pagesize<=0) pagesize=10;
            int totalproducts= await products.CountAsync();
          //  int numberofproductsPerPage =10;
            int countPages= (int)Math.Ceiling((double)totalproducts/pagesize);
            
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

            ViewBag.productIndex=(currentPage -1) *pagesize;
            ViewBag.pagingModel =pagingmodel;
            ViewBag.totalproducts=totalproducts;
            
            
            //do sang View so bai product cua trang truy van hien tai
            var productinPage = await products.Skip((currentPage-1)*pagesize)
                                    .Take(pagesize)
                                    .Include(p=>p.ProductCategoriesProducts)
                                    .ThenInclude(pc=>pc.Category)
                                    .ToListAsync();
            return View(productinPage);
        }

        // GET: product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ProductS == null)
            {
                return NotFound();
            }

            var product = await _context.ProductS
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: product/Create
        public async Task<IActionResult> Create()
        {
           // ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "UserName");

           var categories= await _context.CategoryProducts.ToListAsync();
           ViewData["Categories"]=new MultiSelectList(categories,"Id", "Title");
           return View();
        }

        // product: product/Create
        // To protect from overproducting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIDs,Price")] CreateProductModel product)
        {
            
            if (product.Slug==null)
                {
                  product.Slug = AppUtilities.GenerateSlug(product.Title);
                }

            if (await  _context.ProductS.AnyAsync(p=>p.Slug==product.Slug))
            {
                  ModelState.AddModelError("Slug trùng nhau","Nhập chuỗi Url khác -- trong CSDL đã có slug - Url này rồi");
                  return View(product);
            }
            if (ModelState.IsValid)
            {
                var user= await _userManager.GetUserAsync(this.User);

                product.DateCreated=product.DateUpdated=DateTime.Now;
                product.AuthorId=user.Id;
                
                _context.Add(product);

                // Thêm CategoryID vào bảng productCategory trong CSDL
                if( product.CategoryIDs!=null)
                {
                    foreach (var catID in product.CategoryIDs)
                    {
                        _context.Add( new ProductCategoryProduct()
                        {
                            
                            CategoryID=catID,
                            Product=product
                        });
                    }
                }
                await _context.SaveChangesAsync();
                StatusMessage="Bạn vừa tạo sản phẩm mới: "+ product.Title;
                return RedirectToAction(nameof(Index));
            }
            //ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "UserName", product.AuthorId);
            return View(product);
        }

        // GET: product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ProductS == null)
            {
                return NotFound();
            }
            //var product=await _context.ProductS.FindAsync(id); find async có thể k lấy ra dc pc
            var product= await _context.ProductS.Include(p=>p.ProductCategoriesProducts).FirstOrDefaultAsync(p=>p.ProductId==id);
            if(product==null)
            {
                 return NotFound();
            }

            var productEdit = new CreateProductModel()
            {
                ProductId=product.ProductId,
                Title= product.Title,
                Content=product.Content,
                Description=product.Description,
                Slug=product.Slug,
                Published=product.Published,
                CategoryIDs=product.ProductCategoriesProducts.Select(pc=>pc.CategoryID).ToArray(),
                Price=product.Price

            };
            
           var categories= await _context.CategoryProducts.ToListAsync();
            ViewData["Categories"]=new MultiSelectList(categories,"Id", "Title");
            return View(productEdit);
        }

        // product: product/Edit/5
        // To protect from overproducting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Title,Description,Slug,Content,Published,CategoryIDs,Price,Photos")] CreateProductModel product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }
            var categories= await _context.CategoryProducts.ToListAsync();
            ViewData["Categories"]=new MultiSelectList(categories,"Id", "Title");
            
            if (product.Slug==null)
                {
                  product.Slug = AppUtilities.GenerateSlug(product.Title);
                }

            if (await  _context.ProductS.AnyAsync(p=>p.Slug==product.Slug&&p.ProductId!=id))
            {
                  ModelState.AddModelError("Slug trùng nhau","Nhập chuỗi Url khác -- trong CSDL đã có slug - Url này rồi");
                  return View(product);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productUpdate= await _context.ProductS.Include(p=>p.ProductCategoriesProducts).FirstOrDefaultAsync(p=>p.ProductId==id);
                   if(productUpdate==null)
                   {
                       return NotFound();
                   }
                      productUpdate.Title=product.Title;
                      productUpdate.Description=product.Description;
                      productUpdate.Content=product.Content;
                      productUpdate.Slug=product.Slug;
                      productUpdate.Published=product.Published;
                      productUpdate.DateUpdated=DateTime.Now;
                      productUpdate.Price=product.Price;
                       productUpdate.Photos=product.Photos;
                      //Update product Category
                      if(product.CategoryIDs==null)product.CategoryIDs= new int[]{};
                      var oldCateIds= productUpdate.ProductCategoriesProducts.Select(c=>c.CategoryID).ToArray();
                      var newCateIds= product.CategoryIDs;
                      var removecatIds= from productcat in productUpdate.ProductCategoriesProducts
                                    where (!newCateIds.Contains(productcat.CategoryID))
                                    select productcat;
                     _context.ProductCategoryProducts.RemoveRange(removecatIds);

                     var addcatIds= from CatId in newCateIds
                                     where !oldCateIds.Contains(CatId)
                                     select CatId;
                   foreach (var CatId in addcatIds)
                   {

                        _context.ProductCategoryProducts.Add(new ProductCategoryProduct(){
                            ProductID=id,
                            CategoryID=CatId
                        });
                   }          

                    _context.Update(productUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!productExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage="Bạn vừa cập nhật sản phẩm: " + product.Title;
                return RedirectToAction(nameof(Index));
            }
           // ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", product.AuthorId);
            return View(product);
        }
         [TempData]
         public string StatusMessage {get; set;}
        // GET: product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ProductS == null)
            {
                return NotFound();
            }

            var product = await _context.ProductS
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // product: product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ProductS == null)
            {
                return Problem("Entity set 'AppDbContext.products'  is null.");
            }
            var product = await _context.ProductS.FindAsync(id);
            if (product != null)
            {
                _context.ProductS.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            StatusMessage ="Bạn vừa xóa sản phẩm " +product.Title;
            return RedirectToAction(nameof(Index));
        }

        private bool productExists(int id)
        {
          return _context.ProductS.Any(e => e.ProductId == id);
        }

        //upload one file
        public class UploadOneFile
        {
            [Required (ErrorMessage="Phải chọn file upload")]
            [DataType(DataType.Upload)]
            [FileExtensions(Extensions ="png,jpg,gif,jpeg")]
            [Display(Name ="Chọn file Upload")]
            public IFormFile FileUpload {get;set;}
        }

        [HttpGet]
        public IActionResult UploadPhoto(int id)
        {
            var product= _context.ProductS.Where(e=>e.ProductId==id)
                                 .Include(p=>p.Photos)
                                 .FirstOrDefault();
            if(product==null) return NotFound("Không có sản phẩm");
            ViewData["product"] =product;
            return View( new UploadOneFile());
        }
        
        [HttpPost,ActionName("UploadPhoto")]
        public async Task<IActionResult> UploadPhotoAsync(int id, [Bind("FileUpload")] UploadOneFile f)
        {
             var product= _context.ProductS.Where(e=>e.ProductId==id)
                                 .Include(p=>p.Photos)
                                 .FirstOrDefault();
            if(product==null) return NotFound("Không có sản phẩm");
            ViewData["product"] =product;

            if(f!=null)
            {
                var file1=Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                          +Path.GetExtension(f.FileUpload.FileName);

                var file =Path.Combine("Uploads","Product",file1);
                using (var filestream =new FileStream(file,FileMode.Create))
                {
                    await  f.FileUpload.CopyToAsync(filestream);
                }
            _context.ProductPhotos.Add(new ProductPhoto()
            {
                ProductId=product.ProductId,
                FileName=file1
            });
            await _context.SaveChangesAsync();
            }
            return View( new UploadOneFile());
        }

        //tạo 1 API server trả về Json
        [HttpPost]
        public IActionResult ListPhotos( int id)
        {
             var product= _context.ProductS.Where(e=>e.ProductId==id)
                                 .Include(p=>p.Photos)
                                 .FirstOrDefault();
            if(product==null)
            {
                return Json(
                    new {
                        success =0,
                        message= "Không có sản phẩm"
                    }
                );
            } 
            //ViewData["product"] =product;
             var listphoto= product.Photos.Select(photo=> new {
                          id=photo.Id,
                          path="/contents/Product/"+photo.FileName
             });
             return Json (

                 new {
                     success=1,
                     photos= listphoto
                 }
             );


        }
        [HttpPost]
        public IActionResult DeletePhoto( int id)
        {

            var photo =_context.ProductPhotos.Where(p=>p.Id==id).FirstOrDefault();
                if(photo!=null)
                {
                    _context.Remove(photo);
                    _context.SaveChanges();
                    var fileName="Uploads/Product/"+photo.FileName; // physical path of file photo in server
                    System.IO.File.Delete(fileName);
                }

              return Ok();
        }
        
        [HttpPost,ActionName("UploadPhotoAPI")]
        public async Task<IActionResult> UploadPhotoAPI(int id, [Bind("FileUpload")] UploadOneFile f)
        {
             var product= _context.ProductS.Where(e=>e.ProductId==id)
                                 .Include(p=>p.Photos)
                                 .FirstOrDefault();
            if(product==null) return NotFound("Không có sản phẩm");
           // ViewData["product"] =product;

            if(f!=null)
            {
                var file1=Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                          +Path.GetExtension(f.FileUpload.FileName);

                var file =Path.Combine("Uploads","Product",file1);
                using (var filestream =new FileStream(file,FileMode.Create))
                {
                    await  f.FileUpload.CopyToAsync(filestream);
                }
            _context.ProductPhotos.Add(new ProductPhoto()
            {
                ProductId=product.ProductId,
                FileName=file1
            });
            await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
