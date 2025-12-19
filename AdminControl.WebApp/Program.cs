using AdminControl.BLL.Concrete;
using AdminControl.BLL.Interfaces;
using AdminControl.DAL;
using AdminControl.DALEF.Concrete;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace AdminControl.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =============================================
            // 1. LOGGING (Log4Net)
            // =============================================
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
                loggingBuilder.AddLog4Net("log4net.xml");
            });

            // =============================================
            // 2. DATABASE (Entity Framework Core)
            // =============================================
            var connStr = builder.Configuration.GetConnectionString("AdminControl") 
                ?? throw new InvalidOperationException("Connection string 'AdminControl' not found.");

            builder.Services.AddDbContext<AdminControlContext>(options =>
                options.UseSqlServer(connStr));

            // =============================================
            // 3. DEPENDENCY INJECTION (DAL -> BLL)
            // =============================================
            // DAL Layer
            builder.Services.AddScoped<IRoleDal, RoleDalEf>();
            builder.Services.AddScoped<IUserDal, UserDalEf>();

            // BLL Layer (using full namespace to avoid conflicts with ASP.NET Identity)
            builder.Services.AddScoped<BLL.Interfaces.IRoleManager, BLL.Concrete.RoleManager>();
            builder.Services.AddScoped<IUserManager, UserManager>();
            builder.Services.AddScoped<IAuthManager, AuthManager>();

            // =============================================
            // 4. AUTHENTICATION
            // =============================================
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(2);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

            // =============================================
            // 5. AUTHORIZATION (Policy-based)
            // =============================================
            builder.Services.AddAuthorizationBuilder()
                // Admin-only policy (full access)
                .AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"))
                // Admin or Manager policy (read + limited write)
                .AddPolicy("AdminOrManager", policy =>
                    policy.RequireRole("Admin", "Manager"))
                // Can manage users (create, update, delete)
                .AddPolicy("CanManageUsers", policy =>
                    policy.RequireRole("Admin"))
                // Can edit users (update only, no create/delete)
                .AddPolicy("CanEditUsers", policy =>
                    policy.RequireRole("Admin", "Manager"))
                // Can view users
                .AddPolicy("CanViewUsers", policy =>
                    policy.RequireRole("Admin", "Manager"))
                // Can manage roles (only Admin)
                .AddPolicy("CanManageRoles", policy =>
                    policy.RequireRole("Admin"))
                // Can view roles
                .AddPolicy("CanViewRoles", policy =>
                    policy.RequireRole("Admin", "Manager"))
                // Can view dashboard
                .AddPolicy("CanViewDashboard", policy =>
                    policy.RequireRole("Admin", "Manager"));

            // =============================================
            // 6. MVC
            // =============================================
            builder.Services.AddControllersWithViews();

            // =============================================
            // BUILD APP
            // =============================================
            var app = builder.Build();

            // =============================================
            // MIDDLEWARE PIPELINE
            // =============================================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
