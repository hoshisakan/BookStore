using System.Data;
using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models;

namespace HoshiBook.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private ApplicationDbContext _db;

        public CompanyRepository(ApplicationDbContext db) : base (db)
        {
            _db = db;
        }

        public void Update(Company obj)
        {
            _db.Companies.Update(obj);
        }

        public bool IsExists(string includeProperties, string value)
        {
            if (includeProperties == "Name")
            {
                return _db.Companies.Any(u => u.Name == value);
            }
            else if (includeProperties == "StreetAddress")
            {
                return _db.Companies.Any(u => u.StreetAddress == value);
            }
            else if (includeProperties == "City")
            {
                return _db.Companies.Any(u => u.City == value);
            }
            else if (includeProperties == "State")
            {
                return _db.Companies.Any(u => u.State == value);
            }
            else if (includeProperties == "PostalCode")
            {
                return _db.Companies.Any(u => u.PostalCode == value);
            }
            else if (includeProperties == "PhoneNumber")
            {
                return _db.Companies.Any(u => u.PhoneNumber == value);
            }
            else
            {
                return false;
            }
        }

        public List<Company> GetExistsUsersCompanies(int companyId)
        {
            return (
                from c in _db.Companies
                join u in _db.ApplicationUsers
                on c.Id equals u.CompanyId
                where c.Id == companyId
                select c
            ).ToList();
        }

        public int GetExistsUsersCompaniesCount(int companyId)
        {
            return (
                from c in _db.Companies
                join u in _db.ApplicationUsers
                on c.Id equals u.CompanyId
                where c.Id == companyId
                select c
            ).Count();
        }

        public DataSet ConvertToDataSet(List<Company> data)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            List<string> columnNames = new List<string>()
            {
                "Name", "StreetAddress", "City", "State", "PostalCode", "PhoneNumber"
            };
            dt.TableName = "Companies";
            dt.Columns.Add(columnNames[0], typeof(string));
            dt.Columns.Add(columnNames[1], typeof(string));
            dt.Columns.Add(columnNames[2], typeof(string));
            dt.Columns.Add(columnNames[3], typeof(string));
            dt.Columns.Add(columnNames[4], typeof(string));
            dt.Columns.Add(columnNames[5], typeof(string));

            foreach (var company in data)
            {
                DataRow row = dt.NewRow();
                row[columnNames[0]] = company.Name;
                row[columnNames[1]] = company.StreetAddress;
                row[columnNames[2]] = company.City;
                row[columnNames[3]] = company.State;
                row[columnNames[4]] = company.PostalCode;
                row[columnNames[5]] = company.PhoneNumber;
                dt.Rows.Add(row);
            }
            ds.Tables.Add(dt);
            return ds;
        }
    }
}