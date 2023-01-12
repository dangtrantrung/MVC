using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Contacts
{
  public class Contact 
  {
      [Key]
      public int Id{get;set;}
      [Column(TypeName="nvarchar")]
      [StringLength(50)]
      [Required(ErrorMessage ="Phải nhập {0}")]
      [Display(Name="Họ Tên")]
      public string FullName{get;set;}
         
         [StringLength(100)]
         [Required(ErrorMessage ="Phải nhập {0}")]
         [Display(Name ="Địa chỉ email")]
         [EmailAddress(ErrorMessage ="Phải là {0}")]
      public string Email {get;set;}

      public DateTime DateSent{get;set;}

     [Display(Name ="Nội dung")]
      public string Message {get;set;}
       [StringLength(50)]
       [Phone(ErrorMessage ="Phải là {0}")]
       [Display(Name ="Số điện thoại")]
       [Required(ErrorMessage ="Phải nhập {0}")]
      public string Phone{get;set;}
  }

}