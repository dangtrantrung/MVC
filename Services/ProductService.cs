using App.Models;

namespace App.Service
{
    public class ProductService : List<ProductModel>
    {
        public ProductService()
        {
            this.AddRange(new ProductModel[]
            {
                new ProductModel(){Id=1,Name="I Phone X",Price=1000},
                new ProductModel(){Id=2,Name="SamSung X",Price=800},
                new ProductModel(){Id=3,Name="Nokia 3",Price=600},
                new ProductModel(){Id=4,Name="BK Phone 4",Price=500},
            });
        }
    }
}