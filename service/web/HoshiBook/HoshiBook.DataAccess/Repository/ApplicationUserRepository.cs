using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models;
using HoshiBook.Models.ViewModels.User;

using System.Data;


namespace HoshiBook.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;

        public ApplicationUserRepository(ApplicationDbContext db) : base (db)
        {
            _db = db;
        }

        public bool IsExists(string includeProperties, string value)
        {
            if (includeProperties == "Name")
            {
                return _db.ApplicationUsers.Any(u => u.Name == value);
            }
            else if (includeProperties == "Email")
            {
                return _db.ApplicationUsers.Any(u => u.Email == value);
            }
            else if (includeProperties == "PhoneNumber")
            {
                return _db.ApplicationUsers.Any(u => u.PhoneNumber == value);
            }
            else
            {
                return false;
            }
        }

        public List<ApplicationUser> GetExistsOrderHeadersUsers(string userId)
        {
            return (
                from user in _db.ApplicationUsers
                join orderHeader in _db.OrderHeaders
                on user.Id equals orderHeader.ApplicationUserId
                where user.Id == userId
                select user
            ).ToList();
        }

        public int GetExistsOrderHeadersUsersCount(string userId)
        {
            return (
                from user in _db.ApplicationUsers
                join orderHeader in _db.OrderHeaders
                on user.Id equals orderHeader.ApplicationUserId
                where user.Id == userId
                select user
            ).Count();
        }

        public DataSet ConvertToDataSet(List<UserDetailsVM> data, string? includeProperty = null, bool isDescendingOrder = false)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            List<string> columnNames = new List<string>()
            {
                "Id", "Name", "Email", "PhoneNumber", "StreetAddress",
                "City", "State", "PostalCode", "CompanyName", "IsLockedOut",
                "CreatedAt", "ModifiedAt", "LastLoginTime", "LoginIPv4Address",
                "LastTryLoginTime", "AccessFailedCount"
            };
            dt.TableName = "Users";

            if (includeProperty != null && isDescendingOrder == true)
            {
                switch (includeProperty)
                {
                    case "Name":
                        data = data.OrderByDescending(u => u.Name).ToList();
                        break;
                    case "Email":
                        data = data.OrderByDescending(u => u.Email).ToList();
                        break;
                    case "PhoneNumber":
                        data = data.OrderByDescending(u => u.PhoneNumber).ToList();
                        break;
                    case "StreetAddress":
                        data = data.OrderByDescending(u => u.StreetAddress).ToList();
                        break;
                    case "City":
                        data = data.OrderByDescending(u => u.City).ToList();
                        break;
                    case "State":
                        data = data.OrderByDescending(u => u.State).ToList();
                        break;
                    case "PostalCode":
                        data = data.OrderByDescending(u => u.PostalCode).ToList();
                        break;
                    case "RoleNumber":
                        data = data.OrderByDescending(u => u.RoleNumber).ToList();
                        break;
                    case "CreatedAt":
                        data = data.OrderByDescending(u => u.CreatedAt).ToList();
                        break;
                    case "ModifiedAt":
                        data = data.OrderByDescending(u => u.ModifiedAt).ToList();
                        break;
                    case "LastLoginTime":
                        data = data.OrderByDescending(u => u.LastLoginTime).ToList();
                        break;
                    case "LoginIPv4Address":
                        data = data.OrderByDescending(u => u.LoginIPv4Address).ToList();
                        break;
                }
            }
            else if (includeProperty != null && isDescendingOrder == false)
            {
                switch (includeProperty)
                {
                    case "Name":
                        data = data.OrderBy(u => u.Name).ToList();
                        break;
                    case "Email":
                        data = data.OrderBy(u => u.Email).ToList();
                        break;
                    case "PhoneNumber":
                        data = data.OrderBy(u => u.PhoneNumber).ToList();
                        break;
                    case "StreetAddress":
                        data = data.OrderBy(u => u.StreetAddress).ToList();
                        break;
                    case "City":
                        data = data.OrderBy(u => u.City).ToList();
                        break;
                    case "State":
                        data = data.OrderBy(u => u.State).ToList();
                        break;
                    case "PostalCode":
                        data = data.OrderBy(u => u.PostalCode).ToList();
                        break;
                    case "RoleNumber":
                        data = data.OrderBy(u => u.RoleNumber).ToList();
                        break;
                    case "CreatedAt":
                        data = data.OrderBy(u => u.CreatedAt).ToList();
                        break;
                    case "ModifiedAt":
                        data = data.OrderBy(u => u.ModifiedAt).ToList();
                        break;
                    case "LastLoginTime":
                        data = data.OrderBy(u => u.LastLoginTime).ToList();
                        break;
                    case "LoginIPv4Address":
                        data = data.OrderBy(u => u.LoginIPv4Address).ToList();
                        break;
                }
            }

            foreach (string columnName in columnNames)
            {
                dt.Columns.Add(columnName);
            }

            foreach (var item in data)
            {
                DataRow dr = dt.NewRow();
                dr["Id"] = item.Id;
                dr["Name"] = item.Name;
                dr["Email"] = item.Email;
                dr["PhoneNumber"] = item.PhoneNumber;
                dr["StreetAddress"] = item.StreetAddress;
                dr["City"] = item.City;
                dr["State"] = item.State;
                dr["PostalCode"] = item.PostalCode;
                dr["CompanyName"] = item.CompanyName;
                dr["IsLockedOut"] = item.IsLockedOut;
                dr["CreatedAt"] = item.CreatedAt;
                dr["ModifiedAt"] = item.ModifiedAt;
                dr["LastLoginTime"] = item.LastLoginTime;
                dr["LoginIPv4Address"] = item.LoginIPv4Address;
                dr["LastTryLoginTime"] = item.LastTryLoginTime;
                dr["AccessFailedCount"] = item.AccessFailedCount;
                dt.Rows.Add(dr);
            }
            ds.Tables.Add(dt);
            return ds;
        }

        public DataSet ConvertToDataSet(List<UserImortFormatVM> data, string? includeProperty = null, bool isDescendingOrder = false)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            List<string> columnNames = new List<string>()
            {
                "Email", "Name", "PhoneNumber", "StreetAddress",
                "City", "State", "PostalCode", "Password", "RoleName",
                "CompanyName"
            };
            dt.TableName = "UsersImportFormat";

            foreach (string columnName in columnNames)
            {
                dt.Columns.Add(columnName);
            }

            if (includeProperty != null && isDescendingOrder == true)
            {
                switch (includeProperty)
                {
                    case "Name":
                        data = data.OrderByDescending(u => u.Name).ToList();
                        break;
                    case "Email":
                        data = data.OrderByDescending(u => u.Email).ToList();
                        break;
                    case "PhoneNumber":
                        data = data.OrderByDescending(u => u.PhoneNumber).ToList();
                        break;
                    case "StreetAddress":
                        data = data.OrderByDescending(u => u.StreetAddress).ToList();
                        break;
                    case "City":
                        data = data.OrderByDescending(u => u.City).ToList();
                        break;
                    case "State":
                        data = data.OrderByDescending(u => u.State).ToList();
                        break;
                    case "PostalCode":
                        data = data.OrderByDescending(u => u.PostalCode).ToList();
                        break;
                    case "RoleName":
                        data = data.OrderByDescending(u => u.RoleName).ToList();
                        break;
                    case "CompanyName":
                        data = data.OrderByDescending(u => u.CompanyName).ToList();
                        break;
                }
            }
            else if (includeProperty != null && isDescendingOrder == false)
            {
                switch (includeProperty)
                {
                    case "Name":
                        data = data.OrderBy(u => u.Name).ToList();
                        break;
                    case "Email":
                        data = data.OrderBy(u => u.Email).ToList();
                        break;
                    case "PhoneNumber":
                        data = data.OrderBy(u => u.PhoneNumber).ToList();
                        break;
                    case "StreetAddress":
                        data = data.OrderBy(u => u.StreetAddress).ToList();
                        break;
                    case "City":
                        data = data.OrderBy(u => u.City).ToList();
                        break;
                    case "State":
                        data = data.OrderBy(u => u.State).ToList();
                        break;
                    case "PostalCode":
                        data = data.OrderBy(u => u.PostalCode).ToList();
                        break;
                    case "RoleName":
                        data = data.OrderBy(u => u.RoleName).ToList();
                        break;
                    case "CompanyName":
                        data = data.OrderBy(u => u.CompanyName).ToList();
                        break;
                }
            }

            foreach (var item in data)
            {
                DataRow dr = dt.NewRow();
                dr["Email"] = item.Email;
                dr["Name"] = item.Name;
                dr["PhoneNumber"] = item.PhoneNumber;
                dr["StreetAddress"] = item.StreetAddress;
                dr["City"] = item.City;
                dr["State"] = item.State;
                dr["PostalCode"] = item.PostalCode;
                dr["Password"] = string.Empty;
                dr["RoleName"] = item.RoleName;
                dr["CompanyName"] = item.CompanyName;
                dt.Rows.Add(dr);
            }
            ds.Tables.Add(dt);
            return ds;
        }

        public List<UserLockStatusVM> GetUsersLockStatus(bool isLockedOut)
        {
            if (isLockedOut)
            {
                return (
                    from user in _db.ApplicationUsers
                    join company in _db.Companies
                    on user.CompanyId equals company.Id
                    into groupjoin from b in groupjoin.DefaultIfEmpty()
                    where user.IsLockedOut
                    select new UserLockStatusVM
                    {
                        Id = user.Id,
                        Name = user.Name,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        StreetAddress = user.StreetAddress ?? "",
                        City = user.City ?? "",
                        State = user.State ?? "",
                        PostalCode = user.PostalCode ?? "",
                        CompanyName = b == null ? "" : b.Name,
                        IsLockedOut = user.IsLockedOut,
                        CreatedAt = user.CreatedAt.ToString() ?? "",
                        ModifiedAt = user.ModifiedAt.ToString() ?? "",
                        LastLoginTime = user.LastLoginTime.ToString() ?? "",
                        LoginIPv4Address = user.LoginIPv4Address ?? "",
                        LastTryLoginTime = user.LastTryLoginTime.ToString() ?? "",
                        AccessFailedCount = user.AccessFailedCount
                    }
                ).ToList();
            }
            else
            {
                return (
                    from user in _db.ApplicationUsers
                    join company in _db.Companies
                    on user.CompanyId equals company.Id
                    into groupjoin from b in groupjoin.DefaultIfEmpty()
                    where !user.IsLockedOut
                    select new UserLockStatusVM
                    {
                        Id = user.Id,
                        Name = user.Name,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        StreetAddress = user.StreetAddress ?? "",
                        City = user.City ?? "",
                        State = user.State ?? "",
                        PostalCode = user.PostalCode ?? "",
                        CompanyName = b == null ? "" : b.Name,
                        IsLockedOut = user.IsLockedOut,
                        CreatedAt = user.CreatedAt.ToString() ?? "",
                        ModifiedAt = user.ModifiedAt.ToString() ?? "",
                        LastLoginTime = user.LastLoginTime.ToString() ?? "",
                        LoginIPv4Address = user.LoginIPv4Address ?? "",
                        LastTryLoginTime = user.LastTryLoginTime.ToString() ?? "",
                        AccessFailedCount = user.AccessFailedCount
                    }
                ).ToList();
            }
        }

        public List<UserLockStatusVM> GetUsersLockStatus(string status)
        {
            if (status == "locked")
            {
                return (
                    from user in _db.ApplicationUsers
                    join company in _db.Companies
                    on user.CompanyId equals company.Id
                    into groupjoin from b in groupjoin.DefaultIfEmpty()
                    where user.IsLockedOut
                    select new UserLockStatusVM
                    {
                        Id = user.Id,
                        Name = user.Name,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        StreetAddress = user.StreetAddress ?? "",
                        City = user.City ?? "",
                        State = user.State ?? "",
                        PostalCode = user.PostalCode ?? "",
                        CompanyName = b == null ? "" : b.Name,
                        IsLockedOut = user.IsLockedOut,
                        CreatedAt = user.CreatedAt.ToString() ?? "",
                        ModifiedAt = user.ModifiedAt.ToString() ?? "",
                        LastLoginTime = user.LastLoginTime.ToString() ?? "",
                        LoginIPv4Address = user.LoginIPv4Address ?? "",
                        LastTryLoginTime = user.LastTryLoginTime.ToString() ?? "",
                        AccessFailedCount = user.AccessFailedCount
                    }
                ).ToList();
            }
            else if (status == "unlocked")
            {
                return (
                    from user in _db.ApplicationUsers
                    join company in _db.Companies
                    on user.CompanyId equals company.Id
                    into groupjoin from b in groupjoin.DefaultIfEmpty()
                    where !user.IsLockedOut
                    select new UserLockStatusVM
                    {
                        Id = user.Id,
                        Name = user.Name,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        StreetAddress = user.StreetAddress ?? "",
                        City = user.City ?? "",
                        State = user.State ?? "",
                        PostalCode = user.PostalCode ?? "",
                        CompanyName = b == null ? "" : b.Name,
                        IsLockedOut = user.IsLockedOut,
                        CreatedAt = user.CreatedAt.ToString() ?? "",
                        ModifiedAt = user.ModifiedAt.ToString() ?? "",
                        LastLoginTime = user.LastLoginTime.ToString() ?? "",
                        LoginIPv4Address = user.LoginIPv4Address ?? "",
                        LastTryLoginTime = user.LastTryLoginTime.ToString() ?? "",
                        AccessFailedCount = user.AccessFailedCount
                    }
                ).ToList();
            }
            else
            {
                return (
                    from user in _db.ApplicationUsers
                    join company in _db.Companies
                    on user.CompanyId equals company.Id
                    into groupjoin from b in groupjoin.DefaultIfEmpty()
                    select new UserLockStatusVM
                    {
                        Id = user.Id,
                        Name = user.Name,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        StreetAddress = user.StreetAddress ?? "",
                        City = user.City ?? "",
                        State = user.State ?? "",
                        PostalCode = user.PostalCode ?? "",
                        CompanyName = b == null ? "" : b.Name,
                        IsLockedOut = user.IsLockedOut,
                        CreatedAt = user.CreatedAt.ToString() ?? "",
                        ModifiedAt = user.ModifiedAt.ToString() ?? "",
                        LastLoginTime = user.LastLoginTime.ToString() ?? "",
                        LoginIPv4Address = user.LoginIPv4Address ?? "",
                        LastTryLoginTime = user.LastTryLoginTime.ToString() ?? "",
                        AccessFailedCount = user.AccessFailedCount
                    }
                ).ToList();
            }
        }
    }
}