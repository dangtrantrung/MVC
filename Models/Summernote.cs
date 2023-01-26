namespace App.Models
{
   public class Summernote{
       public Summernote (string summernoteID, bool loadLibrary =true)
       {   
           SummernoteID=summernoteID;
           LoadLibrary =loadLibrary;

       }
       public string SummernoteID {get;set;}
       public bool LoadLibrary {get;set;}
        public int height {get;set;} =120;
         public string toolbar {get;set;} =@"
          [
      ['style', ['style']],
      ['font', ['bold', 'underline', 'clear']],
      ['color', ['color']],
      ['para', ['ul', 'ol', 'paragraph']],
      ['table', ['table']],
      ['insert', ['link', 'picture', 'video','elfinder']],
      ['height],['height']],
      ['view', ['fullscreen', 'codeview', 'help']]
            ]
         ";
   }
}