
using HoshiBook.DataAccess;
using HoshiBook.DataAccess.Repository;
using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBook.Utility;
using HoshiBook.DataAccess.DbInitializer;
using HoshiBookWeb.QuartzPostgreSQLBackupScheduler;
using HoshiBookWeb.Tools;
using HoshiBookWeb.Tools.ProgramInitializerTool;
using HoshiBook.Models;


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using Quartz;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.FileProviders;



//TODO Use Serilog generate log file and write to console.
//TODO 2023-03-04 Write to file and console successfully.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    //TODO Set serilog to write to console and file, read configuration from appsettings.json
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
    );

    //TODO Enable Legacy Timestamp Behavior for PostgreSQL.
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    //TODO Enable DateTime Infinity Conversions for writable timestamp with time zone DateTime to PostgreSQL database.
    AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

    //TODO Add Data Protection for encrypt and decrypt data on linux.
    builder.Services.AddDataProtection().UseCryptographicAlgorithms(
        new AuthenticatedEncryptorConfiguration()
        {
            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
            ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
        }
    );
    
    //TODO Add Kestrel server runtime dotnet core project in linux environment.
    // builder.WebHost.UseKestrel(options =>
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

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    // Add Razor Pages for auto refresh modify razor pages content.
    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

    builder.Services.Configure<IdentityOptions>(options =>
    {
        // Default Lockout settings.
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
            builder.Configuration.GetSection("AccountLockout:DefaultLockoutTimeSpan").Get<int>()
        );
        options.Lockout.MaxFailedAccessAttempts = builder.Configuration.GetSection(
            "AccountLockout:MaxFailedAccessAttempts"
        ).Get<int>();
        options.Lockout.AllowedForNewUsers = true;
    });

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    });

    builder.Services.AddAuthentication()
        .AddFacebook(options =>
        {
            options.AppId = builder.Configuration["Authentication:ExternalLogin:Facebook:Kestrel:AppId"];
            options.AppSecret = builder.Configuration["Authentication:ExternalLogin:Facebook:Kestrel:AppSecret"];
        })
        .AddGoogle(options =>
        {
            options.ClientId = builder.Configuration["Authentication:ExternalLogin:Google:Kestrel:ClientId"];
            options.ClientSecret = builder.Configuration["Authentication:ExternalLogin:Google:Kestrel:ClientSecret"];
        })
        .AddMicrosoftAccount(options =>
        {
            options.ClientId = builder.Configuration["Authentication:ExternalLogin:Microsoft:Kestrel:ClientId"];
            options.ClientSecret = builder.Configuration["Authentication:ExternalLogin:Microsoft:Kestrel:ClientSecret"];
        });

    //TODO Add PostgreSQL database context and connection settting and change default migration save table from 'public' to 'bootstore'.
    builder.Services.AddDbContext<ApplicationDbContext>(
        options => options.UseNpgsql(
            // builder.Configuration.GetConnectionString("DefaultConnection"),
            builder.Configuration.GetConnectionString("LocalTestConnecton"),
            x => x.MigrationsHistoryTable(
                HistoryRepository.DefaultTableName,
                builder.Configuration.GetSection("PostgreSQLConfigure:Schema").Get<string>()
            )
        )
    );
    //TODO add redis cache service
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
        // options.InstanceName = builder.Configuration.GetSection("RedisConfigure:Deployment:InstanceName").Get<string>();
        options.InstanceName = builder.Configuration.GetSection("RedisConfigure:LocalTest:InstanceName").Get<string>();
    });

    builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
    builder.Services.AddIdentity<ApplicationUser,ApplicationRole>().AddDefaultTokenProviders()
        .AddEntityFrameworkStores<ApplicationDbContext>();
    builder.Services.AddSingleton<IEmailSender, EmailSender>();
    //TODO Register ProgramInitializer to services.
    builder.Services.AddScoped<IProgramInitializer, ProgramInitializer>();
    builder.Services.AddScoped<IDbInitializer, DbInitializer>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = $"/Identity/Account/Login";
        options.LogoutPath = $"/Identity/Account/Logout";
        options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(
            builder.Configuration.GetSection("Authentication:Application:Cookie:ExpireTimeSpan").Get<double>()
        );
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;
        // options.Cookie.Name = builder.Configuration.GetSection(
        //     "Authentication:Application:Cookie:Name"
        // ).Get<string>();
    });
    //TODO Add session service.
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(
            builder.Configuration.GetSection(
                "Authentication:Application:Session:IdleTimeout"
            ).Get<double>()
        );
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        // options.Cookie.Name = builder.Configuration.GetSection(
        //     "Authentication:Application:Session:Name"
        // ).Get<string>();
        // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

    builder.Services.AddQuartz(q =>
    {
        q.UseMicrosoftDependencyInjectionJobFactory();
        q.UseSimpleTypeLoader();
        q.UseInMemoryStore();
        q.UseDefaultThreadPool(tp =>
        {
            tp.MaxConcurrency = 10;
        });
        q.ScheduleJob<BackupJob>(trigger => trigger
            .WithIdentity(
                builder.Configuration.GetSection(
                    "Quartz:MainDatabaseBackupJob:Identity"
                ).Get<string>()
            )
            .UsingJobData(
                "backupTarget",
                builder.Configuration.GetSection(
                    "Quartz:MainDatabaseBackupJob:Target"
                ).Get<string>()
            )
            .WithCronSchedule(
                builder.Configuration.GetSection(
                    "Quartz:MainDatabaseBackupJob:Schedule:WeekendCronExpression"
                    // "Quartz:MainDatabaseBackupJob:Schedule:WeekdaysCronExpression"
                    // "Quartz:MainDatabaseBackupJob:Schedule:LocalTestCronExpression"
                ).Get<string>()
            )
        );
        q.ScheduleJob<BackupJob>(trigger => trigger
            .WithIdentity(
                builder.Configuration.GetSection(
                    "Quartz:SecondaryDatabaseBackupJob:Identity"
                ).Get<string>()
            )
            .UsingJobData(
                "backupTarget",
                builder.Configuration.GetSection(
                    "Quartz:SecondaryDatabaseBackupJob:Target"
                ).Get<string>()
            )
            .WithCronSchedule(
                builder.Configuration.GetSection(
                    "Quartz:SecondaryDatabaseBackupJob:Schedule:WeekendCronExpression"
                    // "Quartz:SecondaryDatabaseBackupJob:Schedule:WeekdaysCronExpression"
                    // "Quartz:SecondaryDatabaseBackupJob:Schedule:LocalTestCronExpression"
                ).Get<string>()
            )
        );
    });

    builder.Services.AddQuartzHostedService(qs =>
    {
        qs.WaitForJobsToComplete = true;
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

    //TODO Enable authentication for MVC and Razor Pages authentication.
    app.UseAuthentication();
    //TODO Enable authorization for MVC and Razor Pages authorization.
    app.UseAuthorization();
    app.UseSession();

    string staticFilesPath = string.Empty;

    using (var scope = app.Services.CreateScope())
    {
        var programInitializer = scope.ServiceProvider.GetRequiredService<IProgramInitializer>();
        staticFilesPath = programInitializer.GetStaticFileStoragePath();
    }

    if (!FileTool.CheckDirExists(staticFilesPath))
    {
        FileTool.CreateDirectory(staticFilesPath);
    }

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(staticFilesPath),
        RequestPath = '/' + builder.Configuration.GetSection("StaticFiles:RequestPath").Get<string>()
    });

    app.MapRazorPages();
    app.MapControllerRoute(
        name: "default",
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
}
catch (Exception ex)
{
    Log.Error($"An error occurred while starting the application: {ex.Message}");
}
Log.CloseAndFlush();