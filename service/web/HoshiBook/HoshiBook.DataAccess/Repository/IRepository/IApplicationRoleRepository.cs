using HoshiBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace HoshiBook.DataAccess.Repository.IRepository
{
    public interface IApplicationRoleRepository : IRepository<ApplicationRole>
    {
        bool IsExists(string includeProperties, string value);
    }
}