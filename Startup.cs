using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudyDocker.Services;
using StudyDocker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace StudyDocker
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "myCors";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            ///��ȡRedis��Default������Ϣ
            var section = Configuration.GetSection("Redis:Default");
            ///��ȡ�����ִ�
            string _connectionString = section.GetSection("Connection").Value;
            ///��ȡʵ������
            string _instanceName = section.GetSection("InstanceName").Value;
            ///��ȡ���ݿ�
            int _defaultDB = int.Parse(section.GetSection("DefaultDB").Value ?? "0");
            services.AddControllers();
            ///�м��Redis�����ע��
            services.AddSingleton(new RedisHelper(_connectionString, _instanceName, _defaultDB));
            ///��־׷�ټ�¼�����ע��
            services.AddScoped<IAccessHistoryLogService, AccessHistoryLogService>();
            ///���ݿ����ӷ����ע��
            services.AddDbContext<StudyRedisContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("SqlServer")));
            /*services.AddSession(options =>
                {
                    *//*options.IdleTimeout = TimeSpan.FromMilliseconds(10);*//*
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });*/
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddRazorPages();

            services.AddCors(
                options => options.AddPolicy("myCors",
                    /*new CorsPolicyBuilder("myCorsOptions")
                    ///����ƾ֤
                    .AllowCredentials()
                    /// preflight �Ǽ����������������֮ǰ��Ԥ������
                    .SetPreflightMaxAge(new TimeSpan(0, 10, 0))
                    .WithHeaders(Configuration["Cors:AllowHanders"].Split(";"))
                    .WithMethods("GET", "POST", "PUT")
                    .WithOrigins(Configuration["Cors:AllowOrigins"].Split(";"))
                    .WithExposedHeaders(Configuration["Cors:AllowExposedHanders"].Split(";"))
                    .Build()
                    )*/
                    builder =>
                    {
                        builder
                            ///ʹ��AllowCredentials()ʱorigin����Ϊ��һԭ��
                            .WithOrigins("http://localhost:8088")
                            ///.AllowAnyHeader()
                            .WithHeaders(Configuration["Cors:AllowHanders"].Split(";"))
                            .AllowAnyMethod()
                            ///���ÿ���Cookie
                            .AllowCredentials()
                            ///.WithExposedHeaders(Configuration["Cors:AllowExposedHanders"].Split(";"))
                            ;
                    }
                )
            );

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(configureOptions =>
                {
                    configureOptions.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                });

            /*services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });*/


            /*services.AddCors(opts =>
            {
                opts.AddPolicy("web",
                    new CorsPolicyBuilder()
                    .WithOrigins(Configuration["Data:Cors"].Split(','))
                    .WithMethods("GET", "POST", "DELETE", "OPTIONS", "PUT")
                    .WithHeaders("Content-Type", "Authorization", "Accept", "X-Local-Userid")
                    .AllowCredentials()
                    .SetPreflightMaxAge(new TimeSpan(0, 10, 0))
                    .WithExposedHeaders("WWW-Authenticate", "Server-Authorization")
                    .Build());
            });*/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            /*if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }*/

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //����Https�ض���
            app.UseHttpsRedirection();
            //app.UseStaticFiles();

            app.UseRouting();
            app.UseCors(MyAllowSpecificOrigins);
            ///app.UseCookiePolicy();

            app.UseAuthorization();

            //��UseHttpsRedirection��UseEndpoints����������֮ǰʹ��
            //app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                /*endpoints.MapDefaultControllerRoute();*/
                /*endpoints.MapControllerRoute(name: "Test",
                    pattern: "{controller=Test}/{action=Test1}/{path}/?index={pageIndex}&size={pageSize}");*/
            });


            /*app.UseEndpoints(endpoints =>
            {
                *//*endpoints.MapGet("/api/AccessHistoryLogs/access",
                    context => context.Response.WriteAsync("access"))
                    .RequireCors(MyAllowSpecificOrigins);*//*

                endpoints.MapControllers()
                         .RequireCors(MyAllowSpecificOrigins);

                *//*endpoints.MapGet("/echo2",
                    context => context.Response.WriteAsync("echo2"));

                endpoints.MapRazorPages();*//*
            });*/


        }
    }
}
