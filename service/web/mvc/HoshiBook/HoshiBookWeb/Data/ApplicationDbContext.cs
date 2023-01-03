using Microsoft.EntityFrameworkCore;
using HoshiBookWeb.Models;
namespace HoshiBookWeb.Data;

public class ApplicationDbContext: DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; } = default!;
}