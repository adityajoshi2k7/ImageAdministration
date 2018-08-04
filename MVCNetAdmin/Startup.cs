using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MVCNetAdmin.Models;

namespace MVCNetAdmin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)  //services for rest other envs except staging....can have a separate private method for common services
        {
          
            
            services.AddDbContext<NetAdminContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("NetAdminDB"));
          
                options.EnableSensitiveDataLogging();
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                       
                        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                        options.SlidingExpiration = true;
                        options.LoginPath = "/Login/LoginForm";

                        options.LoginPath = "/Login/LoginForm";

                    });
            services.AddMvc();
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });
        }
        public void ConfigureStagingServices(IServiceCollection services)   //services for staging environment.
        {

           
            services.AddDbContext<NetAdminContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("NetAdminDBStaging"));

                options.EnableSensitiveDataLogging();
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {

                        options.CookieName = "aditya";
                        options.ExpireTimeSpan= TimeSpan.FromMinutes(2);
                        options.SlidingExpiration = true;
                        options.LoginPath = "/Login/LoginForm";
                       

                    });
            services.AddMvc();

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                //app.UseExceptionHandler("/Home/Error");
            }
            var options = new RewriteOptions()
            .AddRedirectToHttps(StatusCodes.Status301MovedPermanently, 44335);
            app.UseRewriter(options);
            app.UseStaticFiles();
            app.UseMiddleware<StackifyMiddleware.RequestTracerMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Login}/{action=LoginForm}/{id?}");
            });
        }
    }
}
