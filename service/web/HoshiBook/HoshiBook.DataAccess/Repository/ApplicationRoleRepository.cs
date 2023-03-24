using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models;


namespace HoshiBook.DataAccess.Repository
{
    public class ApplicationRoleRepository : Repository<ApplicationRole>, IApplicationRoleRepository
    {
        private ApplicationDbContext _db;

        public ApplicationRoleRepository(ApplicationDbContext db) : base (db)
        {
            _db = db;
        }

        public bool IsExists(string includeProperties, string value)
        {
            if (includeProperties == "Name")
            {
                return _db.ApplicationRoles.Any(u => u.Name == value);
            }
            else
            {
                return false;
            }
        }
    }
}