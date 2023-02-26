using HoshiBook.DataAccess;
using HoshiBook.DataAccess.Repository;
using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using HoshiBook.DataAccess.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

// Add Kerstrel configuration
// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenAnyIP(builder.Configuration.GetSection("Deployment:Kestrel:Http:Port").Get<int>());
//     options.ListenAnyIP(builder.Configuration.GetSection("Deployment:Kestrel:Https:Port").Get<int>(), listenOptions =>
//     {
//         listenOptions.UseHttps(
//             builder.Configuration["Deployment:Kestrel:Https:Certificate:Path"],
//             builder.Configuration["Deployment:Kestrel:Https:Certificate:Password"]
//         );
//     });
// });

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(builder.Configuration.GetSection("Deployment:Kestrel:Http:Port").Get<int>());
    options.ListenAnyIP(builder.Configuration.GetSection("Deployment:Kestrel:Https:Port").Get<int>(), listenOptions =>
    {
        listenOptions.UseHttps(
            builder.Configuration["Deployment:Kestrel:Https:Certificate:Path"],
            builder.Configuration["Deployment:Kestrel:Https:Certificate:Password"]
        );
    });
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddAuthentication()
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:Kestrel:AppId"];
        options.AppSecret = builder.Configuration["Authentication:Facebook:Kestrel:AppSecret"];
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:Kestrel:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:Kestrel:ClientSecret"];
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Microsoft:Kestrel:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Microsoft:Kestrel:ClientSecret"];
    });

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsHistoryTable(
            HistoryRepository.DefaultTableName,
            "bookstore"
        )
    )
);
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

SeedDatabase();
app.UseAuthentication();;
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    // pattern: "{controller=Home}/{action=Index}/{id?}");
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}"
);

app.Run();

void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initializer();
    }
}