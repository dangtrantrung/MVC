using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Service;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{  
    [Area("ProductManage")]
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public IActionResult Index()
        {
             // do thiết lập area nên View lưu trữ trong /Areas/AreaName/Views/ControllerName/Action.cshtml
            var products=_productService.OrderBy(p=>p.Name).ToList();
            return View(products); // Areas/ProductManage/Views/Product/Index.cshtml
        }
    }
}