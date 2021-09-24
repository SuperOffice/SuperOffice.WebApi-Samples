using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.SuperOffice;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApplication
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
            services.AddAntiforgery(options =>
            {
                // alway this site to be hosted in an iFrame.
                options.SuppressXFrameOptionsHeader = true;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddSuperOffice(options =>
                {
                    Configuration.Bind("SuperOffice", options);

                    // SaveTokens Summary:
                    //     Defines whether access and refresh tokens should be stored in the Microsoft.AspNetCore.Authentication.AuthenticationProperties
                    //     after a successful authorization. This property is set to false by default to
                    //     reduce the size of the final authentication cookie.
                    options.SaveTokens = true;

                    // IncludeIdTokenAsClaims Summary:
                    //     Gets or sets a value indicating whether user claims will include the SuperOffice
                    //     id_token claims. This will make the cookies even larger.
                    //     This will put the WebApi URL in the claims.
                    options.IncludeIdTokenAsClaims = true;
                });


            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
