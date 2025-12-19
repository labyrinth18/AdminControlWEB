using AutoMapper;
using AdminControl.BLL.Concrete;
using AdminControl.BLL.Interfaces;
using AdminControl.DAL;
using AdminControl.DALEF.Concrete;
using AdminControl.DALEF.MapperProfiles;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AdminControl.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Logging (Teacher's Pattern: Log4Net)
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
                loggingBuilder.AddLog4Net("log4net.xml");
            });

            // 2. AutoMapper (Teacher's Pattern: Using Service Provider & Logger Factory)
            builder.Services.AddSingleton<IMapper>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.ConstructServicesUsing(sp.GetService);
                    // Scans the Assembly where RoleProfile_Back resides (DALEF)
                    cfg.AddMaps(typeof(RoleProfile_Back).Assembly);
                }); // Note: The teacher's code didn't pass loggerFactory here in console, but in WebApp it's good practice. 
                    // However, strictly following the provided Program.cs snippet:
                return config.CreateMapper();
            });

            // 3. Database Connection
            string connStr = builder.Configuration.GetConnectionString("AdminControl") ?? "";

            // 4. DI Registrations (DAL -> BLL -> Web)
            // DAL
            builder.Services.AddTransient<IRoleDal>(sp => new RoleDalEf(connStr, sp.GetRequiredService<IMapper>()))
                            .AddTransient<IUserDal>(sp => new UserDalEf(connStr, sp.GetRequiredService<IMapper>()));

            // BLL
            builder.Services.AddTransient<IRoleManager, RoleManager>()
                            .AddTransient<IAuthManager, AuthManager>();

            // 5. Authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(2);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

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