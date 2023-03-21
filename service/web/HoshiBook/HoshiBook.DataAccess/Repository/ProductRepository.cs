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
    }
}