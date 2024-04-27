using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models;

using System.Data;


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

        public bool IsExists(string includeProperties, string value, int id)
        {
            if (includeProperties == "Title")
            {
                return _db.Products.Where(u => u.Id != id).Any(u => u.Title == value);
            }
            else if (includeProperties == "SKU")
            {
                return _db.Products.Where(u => u.Id != id).Any(u => u.SKU == value);
            }
            else if (includeProperties == "ISBN")
            {
                return _db.Products.Where(u => u.Id != id).Any(u => u.ISBN == value);
            }
            else
            {
                return false;
            }
        }

        public bool IsExists(string includeProperties, string value)
        {
            if (includeProperties == "Title")
            {
                return _db.Products.Any(u => u.Title == value);
            }
            else if (includeProperties == "SKU")
            {
                return _db.Products.Any(u => u.SKU == value);
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

        public List<Product> GetExistsOrderDetailsProducts(int productId)
        {
            return (
                from p in _db.Products
                join o in _db.OrderDetails
                on p.Id equals o.ProductId
                where p.Id == productId
                select p
            ).ToList();
        }
    
        public int GetExistsOrderDetailsProductsCount(int productId)
        {
            return (
                from p in _db.Products
                join o in _db.OrderDetails
                on p.Id equals o.ProductId
                where p.Id == productId
                select p
            ).Count();
        }

        public DataSet ConvertToDataSet(List<Product> data)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            List<string> columnNames = new List<string>()
            {
                "Title", "SKU", "Description", "ISBN", "Author",
                "ListPrice", "Price", "Price50", "Price100", 
                "ImageUrl", "Category", "CoverType"
            };
            dt.TableName = "Products";
            dt.Columns.Add(columnNames[0], typeof(string));
            dt.Columns.Add(columnNames[1], typeof(string));
            dt.Columns.Add(columnNames[2], typeof(string));
            dt.Columns.Add(columnNames[3], typeof(string));
            dt.Columns.Add(columnNames[4], typeof(string));
            dt.Columns.Add(columnNames[5], typeof(double));
            dt.Columns.Add(columnNames[6], typeof(double));
            dt.Columns.Add(columnNames[7], typeof(double));
            dt.Columns.Add(columnNames[8], typeof(double));
            dt.Columns.Add(columnNames[9], typeof(string));
            dt.Columns.Add(columnNames[10], typeof(string));
            dt.Columns.Add(columnNames[11], typeof(string));

            foreach (var product in data)
            {
                DataRow row = dt.NewRow();
                row[columnNames[0]] = product.Title;
                row[columnNames[1]] = product.SKU;
                row[columnNames[2]] = product.Description;
                row[columnNames[3]] = product.ISBN;
                row[columnNames[4]] = product.Author;
                row[columnNames[5]] = product.ListPrice;
                row[columnNames[6]] = product.Price;
                row[columnNames[7]] = product.Price50;
                row[columnNames[8]] = product.Price100;
                row[columnNames[9]] = product.ImageUrl.Split("\\").LastOrDefault()?.Replace(" ", "%20") ?? "default.png";
                row[columnNames[10]] = product.Category.Name;
                row[columnNames[11]] = product.CoverType.Name;
                dt.Rows.Add(row);
            }
            ds.Tables.Add(dt);
            return ds;
        }
    }
}