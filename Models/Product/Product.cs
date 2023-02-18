using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace App.Models.Product
{
    [Table("Product")]
    public class Product
    {
         [Key]
    public int ProductId {set; get;}

    [Required(ErrorMessage = "Phải có tiêu đề sản phẩm")]
    [Display(Name = "Tiêu đề")]
    [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} dài {1} đến {2}")]
    public string Title {set; get;}

    [Display(Name = "Mô tả ngắn")]
    public string Description {set; get;}

    [Display(Name="Chuỗi định danh (url)", Prompt = "Nhập hoặc để trống tự phát sinh theo Title")]
    
    [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} dài {1} đến {2}")]
    [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
    public string Slug {set; get;}

    [Display(Name = "Nội dung")]
    public string Content {set; get;}

    [Display(Name = "Xuất bản")]
    public bool Published {set; get;}

    public List<ProductCategoryProduct>?  ProductCategoriesProducts { get; set; }

    //[Required]
    [Display(Name = "Người đăng sản phẩm")]
    public string? AuthorId {set; get;}

    [ForeignKey("AuthorId")]
    [Display(Name = "Tác giả")]
    public AppUser? Author {set; get;}

    [Display(Name = "Ngày tạo")]
    public DateTime DateCreated {set; get;}

    [Display(Name = "Ngày cập nhật")]
    public DateTime DateUpdated {set; get;}
     [Display(Name = "Giá sản phẩm")]
     [Range(0,int.MaxValue, ErrorMessage = "Phải nhập giá trị từ {2} đến {1}")]
    public double Price {set; get;}
   [Display(Name = "Các hình ảnh sản phẩm")]
    public List<ProductPhoto> Photos {get;set;}


    }
}