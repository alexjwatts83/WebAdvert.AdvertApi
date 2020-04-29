using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertApi.HealthChecks;
using AdvertApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

namespace AdvertApi
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
            services.AddAutoMapper(typeof(Startup));
            services.AddTransient<IAdvertStorageService, DynamoDbAdvertStorageService>();
            services.AddControllers();
            services.AddHealthChecks()
                .AddCheck<StorageHealthCheck>("Storage", timeout: new TimeSpan(0, 1, 0));

            services.AddCors(options =>
            {
                options.AddPolicy("AllOrigin", policy => policy.WithOrigins("*").AllowAnyHeader());
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "Alex Watts Web Advertisement Apis",
                        Version = "version 1",
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                        {
                            Email = "lols@lols.com",
                            Name = "Alex Watts"
                        }
                    }
                );
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseHealthChecks("/health");

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Advert Api");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
