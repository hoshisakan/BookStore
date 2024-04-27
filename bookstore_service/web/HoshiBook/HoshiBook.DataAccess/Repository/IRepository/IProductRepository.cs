using HoshiBook.Models;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace HoshiBook.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product obj);
        bool IsExists(string includeProperties, string value);
        bool IsExists(string includeProperties, string value, int id);
        List<Product> GetExistsOrderDetailsProducts(int productId);
        int GetExistsOrderDetailsProductsCount(int productId);
        DataSet ConvertToDataSet(List<Product> data);
    }
}