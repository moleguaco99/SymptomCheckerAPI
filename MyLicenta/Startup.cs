using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyLicenta.DataMining;
using MyLicenta.DataMining.Algorithms;
using MyLicenta.DataMining.PerformanceMetrics;
using MyLicenta.FileProcessing;
using MyLicenta.Models;

namespace MyLicenta
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
            string connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<MedicalDBContext>(opt => opt.UseSqlServer(connectionString));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddControllers();
            services.AddCors();
            services.AddControllers().AddNewtonsoftJson();

            services.AddScoped<IFileParser, FileParser>();
            services.AddScoped<IApriori, Apriori>();
            services.AddScoped<IKMeans, KMeans>();
            services.AddScoped<IKNearestNeighbors, KNearestNeighbors>();
            services.AddScoped<INaiveBayes, NaiveBayes>();
            services.AddScoped<IAprioriTest, AprioriTest>();
            services.AddScoped<IKMeansTest, KMeansTest>();
            services.AddScoped<IKNearestNeighborsTest, KNearestNeighborsTest>();
            services.AddScoped<INaiveBayesTest, NaiveBayesTest>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
