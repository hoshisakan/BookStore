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
    public interface ICoverTypeRepository : IRepository<CoverType>
    {
        void Update(CoverType obj);
        bool IsExists(string includeProperties, string value);
        List<CoverType> GetExistsProductsCoverTypes(int coverTypeId);
        int GetExistsProductsCoverTypesCount(int coverTypeId);
        DataSet ConvertToDataSet(List<CoverType> data);
    }
}