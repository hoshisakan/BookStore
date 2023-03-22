using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models;
using HoshiBook.Models.ViewModels.User;


namespace HoshiBook.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;

        public ApplicationUserRepository(ApplicationDbContext db) : base (db)
        {
            _db = db;
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
                        IsLockedOut = user.IsLockedOut
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
                        IsLockedOut = user.IsLockedOut
                    }
                ).ToList();
            }
        }
    }
}