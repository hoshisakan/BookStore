using Microsoft.EntityFrameworkCore;
using HoshiBook.Models;
namespace HoshiBook.DataAccess;

public class ApplicationDbContext: DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; } = default!;
    public virtual DbSet<CoverType> CoverTypes { get; set; } = default!;
    public virtual DbSet<Product> Products { get; set; } = default!;
}