using MyCompany.Service;
using Microsoft.EntityFrameworkCore;
using MyCompany.Domain.Repositories.Abstract;
using MyCompany.Domain.Repositories.EntityFramework;
using MyCompany.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;


namespace MyCompany
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //*************
            var builder2 = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder2.Build();
            var mySettingsConfig = new Config();
            configuration.GetSection("Project").Bind(mySettingsConfig);
            Console.WriteLine(configuration.GetConnectionString("ConnectionString"));
            //*******************

            builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer(Config.ConnectionString));
            builder.Services.AddTransient<ITextFieldsRepository, EFTextFieldsRepository>();
            builder.Services.AddTransient<IServiceItemsRepository, EFServiceItemsRepository>();
            builder.Services.AddTransient<DataManager>();


            builder.Services.AddIdentity<IdentityUser, IdentityRole>(opts =>
            {
                //opts.User.RequireUniqueEmail = true;
                opts.Password.RequiredLength = 6;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "myCompanyAuth";
                options.Cookie.HttpOnly = true;
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/accessdenied";
                options.SlidingExpiration = true;
            });

            builder.Services.AddAuthorization(x =>
            {
                x.AddPolicy("AdminArea", policy => { policy.RequireRole("admin"); });
            });

            builder.Services.AddControllersWithViews();
            //builder.Services.AddControllersWithViews(x =>
            //{
            //    x.Conventions.Add(new AdminAreaAuthorization("Admin", "AdminArea"));
            //})
        
            //    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0).AddSessionStateTempDataProvider();



            var app = builder.Build();


            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //---------------------
            //builder.Configuration.AddJsonFile("appsettings.json");
            app.Configuration.Bind("Project", new Config());
            //-------------------------

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            

            //app.MapControllerRoute(
            //    name: "default",
            //    pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute("admin", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

            //*-*--*-*-
            //if (env.IsDevelopment())
            //    app.UseDeveloperExceptionPage();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute("admin", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            //    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            //});

            //*-*-*-*-*-*
            
            app.Run();
        }
    }
}
