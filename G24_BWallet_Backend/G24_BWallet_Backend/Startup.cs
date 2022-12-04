using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Twilio.Http;

namespace G24_BWallet_Backend
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
            string mySqlConnectionStr = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContextPool<MyDBContext>(options => options.UseMySql(mySqlConnectionStr,
                ServerVersion.AutoDetect(mySqlConnectionStr)));
            services.AddControllers();
            services.AddScoped<IAccessRepository, AccessRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IFriendRepository, FriendRepository>();
            services.AddScoped<IReceiptRepository, ReceiptRepository>();
            services.AddScoped<IUserDeptRepository, UserDeptRepository>();
            services.AddScoped<IPaidDebtRepository, PaidDebtRepository>();
            services.AddScoped<IEventUserRepository, EventUserRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<IDebtReceiveDetailRepo, DebtReceiveDetailRepo>();
            services.AddScoped<IActivityRepository, ActivityRepository>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
            {
                option.RequireHttpsMetadata = false;
                option.SaveToken = true;
                option.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["Jwt:Audience"],
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });

            
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader();
                                  });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            /*if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }*/
            app.UseExceptionHandler(c => c.Run(async context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                    var responseError = new Respond<string>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Error = exception.Message,
                        Message = "",
                        Data =  null//exception.StackTrace
                    };
                    await context.Response.WriteAsJsonAsync(responseError);
                })
            );

            app.UseStatusCodePages();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
