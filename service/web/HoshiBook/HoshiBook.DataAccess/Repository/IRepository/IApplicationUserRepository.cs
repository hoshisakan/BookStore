using HoshiBook.Models;
using HoshiBook.Models.ViewModels.User;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace HoshiBook.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        List<UserLockStatusVM> GetUsersLockStatus(bool isLockedOut);
    }
}