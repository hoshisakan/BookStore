using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Models;


namespace HoshiBook.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;

        public ApplicationUserRepository(ApplicationDbContext db) : base (db)
        {
            _db = db;
        }
    }
}