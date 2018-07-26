using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddMvc();
        }
        public void ConfigureStagingServices(IServiceCollection services)   //services for staging environment.
        {

           
            services.AddDbContext<NetAdminContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("NetAdminDBStaging"));

                options.EnableSensitiveDataLogging();
            });
            services.AddMvc();

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseMiddleware<StackifyMiddleware.RequestTracerMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
