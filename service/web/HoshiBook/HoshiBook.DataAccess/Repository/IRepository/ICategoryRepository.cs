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
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category obj);
        bool IsExists(string includeProperties, string value);
        List<Category> GetExistsProductsCategories(int categoryId);
        int GetExistsProductsCategoriesCount(int categoryId);
        DataSet ConvertToDataSet(List<Category> data);
    }
}