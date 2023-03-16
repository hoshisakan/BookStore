using HoshiBook.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace HoshiBook.DataAccess;

public class ApplicationDbContext: IdentityDbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; } = default!;
    public virtual DbSet<CoverType> CoverTypes { get; set; } = default!;
    public virtual DbSet<Product> Products { get; set; } = default!;
    public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; } = default!;
    public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; } = default!;
    public virtual DbSet<Company> Companies { get; set; } = default!;
    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; } = default!;
    public virtual DbSet<OrderHeader> OrderHeaders { get; set; } = default!;
    public virtual DbSet<OrderDetail> OrderDetails { get; set; } = default!;
    


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //TODO Specify create table schema name.
        modelBuilder.HasDefaultSchema("bookstore");

        //TODO Set up one-to-many relationship between ApplicationUser and Company.
        modelBuilder.Entity<Company>(
            builder =>
            {
                builder.HasMany(c => c.ApplicationUsers)
                    .WithOne(u => u.Company)
                    .HasForeignKey(u => u.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
            }
        );
    }
}