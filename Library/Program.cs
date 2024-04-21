using Library.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Reflection;

namespace Library
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("LibraryDB"));
            }); //add db

            builder.Services.AddHostedService<ScheduledJob>();
            builder.Services.AddScoped<ReturnDatePassWarning>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                // If the LoginPath isn't set, ASP.NET Core defaults the path to /Account/Login.
                options.LoginPath = "/Account/Login"; // Set your login path here
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            builder.Services.AddIdentity<LibraryUser, IdentityRole>(
                options => {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 7;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredUniqueChars = 1;
                })               
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddTransient<ISenderEmail, EmailSender>();

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.AddMvc()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options => {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                    {
                        var assemblyName = new AssemblyName(typeof(SharedResource).GetTypeInfo().Assembly.FullName!);
                        return factory.Create(nameof(SharedResource), assemblyName.Name!);
                    };
                });

            var app = builder.Build();

            var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("tr-TR") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Books}/{action=Index}/{id?}");
            app.MapRazorPages();
            app.Run();
        }

        

    }
}
