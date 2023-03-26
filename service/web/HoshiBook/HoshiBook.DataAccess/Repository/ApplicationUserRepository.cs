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

        public DataSet ConvertToDataSet(List<UserDetailsVM> data)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            List<string> columnNames = new List<string>()
            {
                "Id", "Name", "Email", "PhoneNumber", "StreetAddress",
                "City", "State", "PostalCode", "CompanyName", "IsLockedOut",
                "CreatedAt", "ModifiedAt", "LastLoginTime", "LoginIPv4Address"
            };
            dt.TableName = "Users";

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
                dt.Rows.Add(dr);
            }
            ds.Tables.Add(dt);
            return ds;
        }

        public DataSet ConvertToDataSet(List<UserImortFormatVM> data)
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
                        LastLoginTime = user.LastLoginTime.ToString() ?? ""
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
                        LastLoginTime = user.LastLoginTime.ToString() ?? ""
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
                        LoginIPv4Address = user.LoginIPv4Address ?? ""
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
                        LoginIPv4Address = user.LoginIPv4Address ?? ""
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
                        LoginIPv4Address = user.LoginIPv4Address ?? ""
                    }
                ).ToList();
            }
        }
    }
}