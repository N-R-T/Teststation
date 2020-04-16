using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Teststation.Models;

namespace Teststation
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Database>(x =>
            {
                x.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<Database>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("LoggedIn",
                     policy => policy.RequireRole(Consts.Admin, Consts.Candidate));
            });
                       

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = Consts.minimalPasswordLength;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                options.Lockout.MaxFailedAccessAttempts = 50;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyz" +
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "áéíóúýâêîôûàèìòù" +
                "ÁÉÍÓÚÝÂÊÎÔÛÀÈÌÒÙ" +
                "´`^?!ß<>,;:*~(){}[]§$-._@+" +
                "0123456789";
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(600);                

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddSession(options =>
            {
                options.Cookie.Expiration = TimeSpan.FromMinutes(600);
                options.IdleTimeout = TimeSpan.FromMinutes(600);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            CreateRoles(serviceProvider).Wait();
        }
            private async Task CreateRoles(IServiceProvider serviceProvider)
            {
                //adding custom roles
                var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var UserManager = serviceProvider.GetRequiredService<UserManager<User>>();
                string[] roleNames = { Consts.Admin, Consts.Candidate };
                IdentityResult roleResult;

                foreach (var roleName in roleNames)
                {
                    //creating the roles and seeding them to the database
                    var roleExist = await RoleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

            //creating a super user who could maintain the web app
            var poweruser = new User
            {
                UserName = Configuration.GetSection("UserSettings")["UserName"],
                Email = Configuration.GetSection("UserSettings")["UserEmail"],
                DayOfLastActivity = DateTime.Now
                };

                string UserPassword = Configuration.GetSection("UserSettings")["UserPassword"];
                var _user = await UserManager.FindByEmailAsync(Configuration.GetSection("UserSettings")["UserEmail"]);

                if (_user == null)
                {
                    var createPowerUser = await UserManager.CreateAsync(poweruser, UserPassword);
                    if (createPowerUser.Succeeded)
                    {
                        await UserManager.AddToRoleAsync(poweruser, Consts.Admin);
                    }
                }
            }
        }
    }
