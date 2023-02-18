using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace App.Models.Product
{
    [Table("ProductPhoto")]
    public class ProductPhoto
    {
    [Key]
    public int Id {set; get;}
    //abc.png
    //->/contents//Product/abc.png
    public string FileName {set; get;}

    public int ProductId {set;get;}

    [ForeignKey("ProductId")]

    public Product Product {set;get;}

    
    }
}