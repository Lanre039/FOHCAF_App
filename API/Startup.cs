﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Core;
using API.Core.Models;
using API.Extension;
using API.Persistence;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace api
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
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IFOHCAFRepository, FOHCAFRepository>();
            services.AddScoped<IPhotoRepository, PhotoRepository>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddAutoMapper();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<PhotoSettings>(Configuration.GetSection("PhotoSettings"));
            services.AddDbContext<FOHCAFDbContext>(options => {
                string connectionString;
                var environmentVariable = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONEMT");
                    connectionString = Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
            });
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
             services.AddAuthentication(auth => {
               auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
               auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x => {
               x.RequireHttpsMetadata = false;
               x.SaveToken = true;
               x.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(key),
                   ValidateIssuer = false,
                   ValidateAudience = false
               };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials());
            app.UseMvc();
            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
