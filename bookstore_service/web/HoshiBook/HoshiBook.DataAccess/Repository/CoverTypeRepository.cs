using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models;

using System.Data;


namespace HoshiBook.DataAccess.Repository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private ApplicationDbContext _db;

        public CoverTypeRepository(ApplicationDbContext db) : base (db)
        {
            _db = db;
        }

        public void Update(CoverType obj)
        {
            _db.CoverTypes.Update(obj);
        }

        public bool IsExists(string includeProperties, string value)
        {
            if (includeProperties == "Name")
            {
                return _db.CoverTypes.Any(u => u.Name == value);
            }
            else
            {
                return false;
            }
        }

        public List<CoverType> GetExistsProductsCoverTypes(int coverTypeId)
        {
            return (
                from c in _db.CoverTypes
                join p in _db.Products
                on c.Id equals p.CoverTypeId
                where c.Id == coverTypeId
                select c
            ).ToList();
        }

        public int GetExistsProductsCoverTypesCount(int coverTypeId)
        {
            return (
                from c in _db.CoverTypes
                join p in _db.Products
                on c.Id equals p.CoverTypeId
                where c.Id == coverTypeId
                select c
            ).Count();
        }

        public DataSet ConvertToDataSet(List<CoverType> data)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            List<string> columnNames = new List<string>()
            {
                "Name"
            };
            dt.TableName = "CoverTypes";
            dt.Columns.Add(columnNames[0], typeof(string));

            foreach (var coverType in data)
            {
                DataRow row = dt.NewRow();
                row[columnNames[0]] = coverType.Name;
                dt.Rows.Add(row);
            }
            ds.Tables.Add(dt);
            return ds;
        }
    }
}