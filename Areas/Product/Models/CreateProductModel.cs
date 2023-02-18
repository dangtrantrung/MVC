using System.ComponentModel.DataAnnotations;
using App.Models.Product;

namespace App.Areas.Product.Models
{
       public class CreateProductModel:App.Models.Product.Product{
           [Display(Name="Chuyên mục Sản phẩm")]
           public int[]? CategoryIDs {get;set;}
       }
}