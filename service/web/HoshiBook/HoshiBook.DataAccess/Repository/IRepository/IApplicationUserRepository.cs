using HoshiBook.Models;
using HoshiBook.Models.ViewModels.User;

using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace HoshiBook.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        bool IsExists(string includeProperties, string value);
        List<ApplicationUser> GetExistsOrderHeadersUsers(string userId);
        int GetExistsOrderHeadersUsersCount(string userId);
        DataSet ConvertToDataSet(List<UserDetailsVM> data, string? includeProperty = null, bool isDescendingOrder = false);
        DataSet ConvertToDataSet(List<UserImortFormatVM> data, string? includeProperty = null, bool isDescendingOrder = false);
        List<UserLockStatusVM> GetUsersLockStatus(bool isLockedOut);
        List<UserLockStatusVM> GetUsersLockStatus(string status);
    }
}