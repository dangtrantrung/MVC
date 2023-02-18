using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Models.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Product.Controllers
{
    [Area("Product")]
    public class ViewProductController : Controller
    {

        private readonly ILogger<ViewProductController> _logger;
        private readonly AppDbContext _context;
        private readonly CartService _cartservice;
        

        public ViewProductController(ILogger<ViewProductController> logger, AppDbContext context, CartService cartService)
        {
            _logger = logger;
            _context = context;
            _cartservice=cartService;
        }
        /* "/product/"
        "/product/{categoryslug?}" */
        [Route("/product/{categoryslug?}")]
        public IActionResult Index(string categoryslug, [FromQuery(Name="p")]int currentPage, int pagesize)
        {
            //return Content(categoryslug);
            var categories =GetCategories();
            ViewBag.categories=categories;
            ViewBag.categoryslug=categoryslug;


            CategoryProduct category =null;
            if(!string.IsNullOrEmpty(categoryslug))
            {
                category=_context.CategoryProducts.Where(c=>c.Slug==categoryslug)
                               .Include(c=>c.CategoryChildren)
                               .FirstOrDefault();
                if(category==null)
                {
                    return NotFound("Không thấy category này trong CSDL");
                }
                ViewBag.category=category;
            }

            var products =_context.ProductS
                              .Include(p=>p.Author)
                              .Include(p=>p.Photos)
                              .Include(p=>p.ProductCategoriesProducts)
                              .ThenInclude(p=>p.Category)
                              .AsQueryable();
           products=products.OrderByDescending(p=>p.DateUpdated);
          if(category!=null)
            {
               var ids= new List<int>();
                category.ChildrenCategoryIDS(ids,null);
                ids.Add(category.Id);
                products=products.Where(p=>p.ProductCategoriesProducts.Where(pc=>ids.Contains(pc.CategoryID)).Any());
            }           
                              

            int totalProducts= products.Count();
            if(pagesize<=0) pagesize=10;
          //  int numberofproductsPerPage =10;
            int countPages= (int)Math.Ceiling((double)totalProducts/pagesize);
            
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

            //do sang View so product cua trang truy van hien tai
            var productinPage = products.Skip((currentPage-1)*pagesize)
                                    .Take(pagesize);
                                    

            //ViewBag.productIndex=(currentPage -1) *pagesize;
            ViewBag.pagingModel =pagingmodel;
            ViewBag.totalProducts=totalProducts;   
            ViewBag.category=category;         

                                
           return View(productinPage.ToList());
        }

        [Route("/product/{productslug}.html")]
        public IActionResult Detail(string productslug)
        {

             //return Content(categoryslug);
            var categories =GetCategories();
            ViewBag.categories=categories;
            var product =_context.ProductS.Where(p=>p.Slug==productslug)
                                    .Include(p=>p.Author)
                                    .Include(p=>p.Photos)
                                    .Include(p=>p.ProductCategoriesProducts)
                                    .ThenInclude(pc=>pc.Category).FirstOrDefault();
            if(product==null)
            {
                return NotFound("Không thấy bài viết");
            }
            CategoryProduct category =product.ProductCategoriesProducts.FirstOrDefault()?.Category;
            ViewBag.category=category;

            //lấy ra 5 bài viết gần nhất
            var otherproducts= _context.ProductS.Where(p=>p.ProductCategoriesProducts.Any(c=>c.Category.Id==category.Id))
                                          .Where(p=>p.ProductId!=product.ProductId)
                                          .OrderByDescending(p=>p.DateUpdated)
                                          .Take(5);
            ViewBag.otherproducts=otherproducts;
            
            return View(product);

        }

        private List<CategoryProduct> GetCategories()
        {
            var categories=_context.CategoryProducts
                           .Include(c=>c.CategoryChildren)
                           .AsEnumerable()
                           .Where(c=>c.ParentCategory==null)
                           .ToList();
            return categories;
        }
        /// Thêm sản phẩm vào cart
[Route ("addcart/{productid:int}", Name = "addcart")]
public IActionResult AddToCart ([FromRoute] int productid) {

    var product = _context.ProductS
        .Where (p => p.ProductId == productid)
        .FirstOrDefault ();
    if (product == null)
        return NotFound ("Không có sản phẩm");

    // Xử lý đưa vào Cart ...
    var cart = _cartservice.GetCartItems();
    var cartitem = cart.Find (p => p.product.ProductId == productid);
    if (cartitem != null) {
        // Đã tồn tại, tăng thêm 1
        cartitem.quantity++;
    } else {
        //  Thêm mới
        cart.Add (new CartItem () { quantity = 1, product = product });
    }

    // Lưu cart vào Session
    _cartservice.SaveCartSession (cart);
    // Chuyển đến trang hiển thị Cart
    return RedirectToAction (nameof (Cart));

}
   // Hiện thị giỏ hàng
[Route ("/cart", Name = "cart")]
public IActionResult Cart () {
    return View (_cartservice.GetCartItems());
}

/// Cập nhật
[Route ("/updatecart", Name = "updatecart")]
[HttpPost]
public IActionResult UpdateCart ([FromForm] int productid, [FromForm] int quantity) {
    // Cập nhật Cart thay đổi số lượng quantity ...
    var cart = _cartservice.GetCartItems ();
    var cartitem = cart.Find (p => p.product.ProductId == productid);
    if (cartitem != null) {
        // Đã tồn tại, tăng thêm 1
        cartitem.quantity = quantity;
    }
    _cartservice.SaveCartSession (cart);
    // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
    return Ok();
}

/// xóa item trong cart
[Route ("/removecart/{productid:int}", Name = "removecart")]
public IActionResult RemoveCart ([FromRoute] int productid) {
    var cart = _cartservice.GetCartItems ();
    var cartitem = cart.Find (p => p.product.ProductId == productid);
    if (cartitem != null) {
        // Đã tồn tại, tăng thêm 1
        cart.Remove(cartitem);
    }

    _cartservice.SaveCartSession (cart);
    return RedirectToAction (nameof (Cart));
}

//gui don hang
[Route ("/checkout")]
public IActionResult Checkout()
{
    var cart =_cartservice.GetCartItems();
    // xử lý gửi email cho khách hàng, xd model lưu trữ thông tin giỏ hàng
     //....
      _cartservice.ClearCart();

    return Content("Đã gửi đơn hàng");
}

    }
    //git commit -m"View product with Breadcrum Media Object Bootstrap 5"
}