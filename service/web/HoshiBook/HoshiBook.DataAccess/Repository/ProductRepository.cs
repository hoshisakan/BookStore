using System.Data;
using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models;

namespace HoshiBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base (db)
        {
            _db = db;
        }

        public void Update(Product obj)
        {
            var objFromDb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Title = obj.Title;
                objFromDb.ISBN = obj.ISBN;
                objFromDb.Price = obj.Price;
                objFromDb.Price50 = obj.Price50;
                objFromDb.Price100 = obj.Price100;
                objFromDb.ListPrice = obj.ListPrice;
                objFromDb.Author = obj.Author;
                objFromDb.CategoryId = obj.CategoryId;
                objFromDb.CoverTypeId = obj.CoverTypeId;
                objFromDb.Description = obj.Description;
                if (obj.ImageUrl != null)
                {
                    objFromDb.ImageUrl = obj.ImageUrl;
                }
                _db.Products.Update(objFromDb);
            }
        }

        public bool IsExists(string includeProperties, string value)
        {
            if (includeProperties == "Title")
            {
                return _db.Products.Any(u => u.Title == value);
            }
            else if (includeProperties == "ISBN")
            {
                return _db.Products.Any(u => u.ISBN == value);
            }
            else
            {
                return false;
            }
        }

        public List<Product> GetExistsOrderDetailsProducts(int id)
        {
            return (
                from p in _db.Products
                join o in _db.OrderDetails
                on p.Id equals o.ProductId
                where p.Id == id
                select p
            ).ToList();
        }
    
        public DataSet ConvertToDataSet(List<Product> data)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            List<string> columnNames = new List<string>()
            {
                "Title", "Description", "ISBN", "Author", "ListPrice", "Price", "Price50", "Price100", "ImageUrl",
                "Category", "CoverType"
            };
            dt.TableName = "Products";
            dt.Columns.Add(columnNames[0], typeof(string));
            dt.Columns.Add(columnNames[1], typeof(string));
            dt.Columns.Add(columnNames[2], typeof(string));
            dt.Columns.Add(columnNames[3], typeof(string));
            dt.Columns.Add(columnNames[4], typeof(double));
            dt.Columns.Add(columnNames[5], typeof(double));
            dt.Columns.Add(columnNames[6], typeof(double));
            dt.Columns.Add(columnNames[7], typeof(double));
            dt.Columns.Add(columnNames[8], typeof(string));
            dt.Columns.Add(columnNames[9], typeof(string));
            dt.Columns.Add(columnNames[10], typeof(string));

            foreach (var product in data)
            {
                DataRow row = dt.NewRow();
                row[columnNames[0]] = product.Title;
                row[columnNames[1]] = product.Description;
                row[columnNames[2]] = product.ISBN;
                row[columnNames[3]] = product.Author;
                row[columnNames[4]] = product.ListPrice;
                row[columnNames[5]] = product.Price;
                row[columnNames[6]] = product.Price50;
                row[columnNames[7]] = product.Price100;
                row[columnNames[8]] = product.ImageUrl;
                row[columnNames[9]] = product.Category.Name;
                row[columnNames[10]] = product.CoverType.Name;
                dt.Rows.Add(row);
            }
            ds.Tables.Add(dt);
            return ds;
        }
    }
}