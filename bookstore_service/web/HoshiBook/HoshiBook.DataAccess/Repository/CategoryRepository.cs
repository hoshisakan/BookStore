using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models;

using System.Data;


namespace HoshiBook.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) : base (db)
        {
            _db = db;
        }

        public void Update(Category obj)
        {
            _db.Categories.Update(obj);
        }

        public bool IsExists(string includeProperties, string value)
        {
            if (includeProperties == "Name")
            {
                return _db.Categories.Any(u => u.Name == value);
            }
            else if (includeProperties == "DisplayOrder")
            {
                return _db.Categories.Any(u => u.DisplayOrder == int.Parse(value));
            }
            else
            {
                return false;
            }
        }

        public List<Category> GetExistsProductsCategories(int categoryId)
        {
            return (
                from c in _db.Categories
                join p in _db.Products
                on c.Id equals p.CategoryId
                where c.Id == categoryId
                select c
            ).ToList();
        }

        public int GetExistsProductsCategoriesCount(int categoryId)
        {
            return (
                from c in _db.Categories
                join p in _db.Products
                on c.Id equals p.CategoryId
                where c.Id == categoryId
                select c
            ).Count();
        }

        public DataSet ConvertToDataSet(List<Category> data)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            List<string> columnNames = new List<string>()
            {
                "Name", "DisplayOrder"
            };
            dt.TableName = "Categories";
            dt.Columns.Add(columnNames[0], typeof(string));
            dt.Columns.Add(columnNames[1], typeof(int));

            foreach (var category in data)
            {
                DataRow row = dt.NewRow();
                row[columnNames[0]] = category.Name;
                row[columnNames[1]] = category.DisplayOrder;
                dt.Rows.Add(row);
            }
            ds.Tables.Add(dt);
            return ds;
        }
    }
}