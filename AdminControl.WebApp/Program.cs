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

            // Add logging with log4net
            builder.Services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddLog4Net("log4net.xml");
            });

            // Configure AutoMapper
            builder.Services.AddSingleton<IMapper>(sp =>
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.ConstructServicesUsing(sp.GetService);
                    cfg.AddMaps(typeof(RoleProfile_Back).Assembly);
                });

                return config.CreateMapper();
            });

            string connStr = builder.Configuration.GetConnectionString("AdminControl") ?? "";

            // DAL registrations (teacher's style)
            builder.Services.AddTransient<IRoleDal>(sp => new RoleDalEf(connStr, sp.GetRequiredService<IMapper>()))
                            .AddTransient<IUserDal>(sp => new UserDalEf(connStr, sp.GetRequiredService<IMapper>()));

            // BL registrations
            builder.Services.AddTransient<IRoleManager, RoleManager>()
                            .AddTransient<IAuthManager, AuthManager>();

            // Configure Authentication
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

            // Configure the HTTP request pipeline.
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
