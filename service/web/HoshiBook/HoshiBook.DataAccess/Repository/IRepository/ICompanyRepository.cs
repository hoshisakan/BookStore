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
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company obj);
        bool IsExists(string includeProperties, string value);
        List<Company> GetExistsUsersCompanies(int companyId);
        int GetExistsUsersCompaniesCount(int companyId);
        DataSet ConvertToDataSet(List<Company> data);
    }
}