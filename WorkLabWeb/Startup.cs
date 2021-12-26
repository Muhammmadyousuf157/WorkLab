using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using WorkLabWeb.Hubs;

namespace WorkLabWeb
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
			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.Cookie.HttpOnly = true;
					options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
					options.Cookie.SameSite = SameSiteMode.Lax;
					options.Cookie.IsEssential = true;
					options.Cookie.Name = "WorkLab.AuthCookieAspNetCore";
					options.LoginPath = "/Users/Account/SignIn";
					options.LogoutPath = "/Users/Account/Logout";
				});

			services.AddAutoMapper(typeof(Startup));

			services
				.AddFluentEmail(Configuration["EmailSettings:Sender"])
				.AddRazorRenderer()
				.AddSmtpSender(
					Configuration["EmailSettings:Host"],
					int.Parse(Configuration["EmailSettings:Port"]),
					Configuration["EmailSettings:Username"],
					Configuration["EmailSettings:Password"]
				);

			services.AddSignalR(o =>
			{
				o.MaximumReceiveMessageSize = null;
				o.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
				o.KeepAliveInterval = TimeSpan.FromSeconds(30);
			}).AddMessagePackProtocol();

			services.AddControllersWithViews(options => options.Filters.Add(new AuthorizeFilter()));
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
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseCookiePolicy();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "areas",
					pattern: "{area}/{controller}/{action}/{id?}");

				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");

				endpoints.MapHub<SessionHub>("/sessionHub", options  =>
				{
					options.ApplicationMaxBufferSize = 0;
					options.TransportMaxBufferSize = 0;
					options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
				});
			});
		}
	}
}