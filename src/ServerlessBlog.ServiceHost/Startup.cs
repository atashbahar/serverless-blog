using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerlessBlog.Application;

namespace ServerlessBlog.ServiceHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.Configure<BlogSettings>(Configuration.GetSection("Blog"));

            services.AddScoped<IBlogDataProvider, BlogDataProvider>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("Not found", "404", new {controller = "Blog", action = "NotFound"});
                endpoints.MapControllerRoute("Error", "error", new {controller = "Blog", action = "Error"});
                endpoints.MapControllerRoute("Posts", "", new { controller = "Blog", action = "Posts", page = 1 });
                endpoints.MapControllerRoute("Posts with page", "posts/page-{page}",
                    new {controller = "Blog", action = "Posts"}, new {page = @"\d+"});
                endpoints.MapControllerRoute("Post", "post/{id}", new { controller = "Blog", action = "Post" });
            });
        }
    }
}
