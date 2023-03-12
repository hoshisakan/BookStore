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
    }
}