using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicCollaborationManager.Data;
using MusicCollaborationManager.Services.Concrete;
using MusicCollaborationManager.Services.Abstract;
using SpotifyAPI.Web;
namespace MCM;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicCollaborationManager.Data;
using MusicCollaborationManager.Models;
using MusicCollaborationManager.Utilities;
using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static SpotifyAPI.Web.Scopes;

public class Program
{

    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        string clientID = builder.Configuration["SpotifyClientID"];
        string clientSecret = builder.Configuration["SpotifySecret"];

        builder.Services.AddControllersWithViews();
        var MCMconnectionString = builder.Configuration.GetConnectionString("MCMConnection");
        builder.Services.AddDbContext<MCMDbContext>(options => options
                                    .UseLazyLoadingProxies()
                                    .UseSqlServer(MCMconnectionString));

        var connectionString = builder.Configuration.GetConnectionString("AuthenticationConnection") ?? throw new InvalidOperationException("Connection string 'AuthenticationConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()                           //enables roles, ie admin
            .AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<ISpotifyVisitorService, SpotifyVisitorService>(s => new SpotifyVisitorService(clientID, clientSecret));
        
        builder.Services.AddSingleton(SpotifyClientConfig.CreateDefault());
        // builder.Services.AddScoped<SpotifyClientBuilder>();

        builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Spotify", policy =>
                {
                    policy.AuthenticationSchemes.Add("Spotify");
                    policy.RequireAuthenticatedUser();
                });
            });
        builder.Services
          .AddAuthentication(options =>
          {
              options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
          })
          .AddCookie(options =>
          {
              options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
          })
          .AddSpotify(options =>
          {
              options.ClientId = clientID;
              options.ClientSecret = clientSecret;
              options.CallbackPath = "/api/callback"; // endpoint for us to recieve the callback
              options.SaveTokens = true;

              var scopes = new List<string> {
                    UserReadEmail, UserReadPrivate, PlaylistReadPrivate, PlaylistReadCollaborative, PlaylistModifyPrivate, PlaylistModifyPublic
            };
              options.Scope.Add(string.Join(",", scopes));
          });



        builder.Services.AddSwaggerGen();
        var app = builder.Build();

        //After build has been called and before run, configure for auth seed data
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var config = app.Services.GetService<IConfiguration>();
                var testUserPw = config["SeedUserPw"];
                var adminPw = config["SeedAdminPw"];

                SeedUsers.Initialize(services, SeedData.UserSeedData, testUserPw).Wait();
                SeedUsers.InitializeAdmin(services, "admin@example.com", "admin", adminPw, "The", "Admin").Wait();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occured seeding the DB");
            }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();

            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();

    }
}

