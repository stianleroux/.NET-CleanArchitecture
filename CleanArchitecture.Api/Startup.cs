namespace CleanArchitecture.Api
{
    using System.IO;
    using System.Threading.Tasks;
    using CleanArchitecture.Common;
    using CleanArchitecture.Common.Logging;
    using CleanArchitecture.Core;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        public Startup(IHostEnvironment env, IConfiguration configuration)
        {
            this.CurrentEnvironment = env;

            this.Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            this.Configuration.Bind("AppSettings", new AppSettings());
        }

        public IConfiguration Configuration { get; }

        public IHostEnvironment CurrentEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "CleanArchitecture Access API";
                    document.Info.Description = "Access";
                    document.Info.TermsOfService = "None";
                    /*document.Info.Contact = new NSwag.OpenApiContact
                     {
                         Name = "Shayne Boyer",
                         Email = string.Empty,
                         Url = "https://twitter.com/spboyer"
                     };
                     document.Info.License = new NSwag.OpenApiLicense
                     {
                         Name = "Use under LICX",
                         Url = "https://example.com/license"
                     };*/
                };


                /* var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                 var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                 c.IncludeXmlComments(xmlPath);*/
            });

            // Add DbContext using SQL Server Provider
            /*services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlServer(this.Configuration.GetConnectionString("Database")));*/

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
            services.AddResponseCompression();

            services.AddCors(options =>
            {
                options.AddPolicy(
                    "CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            /*  services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                  .AddJwtBearer(options =>
                  {
                      options.TokenValidationParameters = new TokenValidationParameters
                      {
                          ValidateIssuer = true,
                          ValidateAudience = true,
                          ValidateLifetime = true,
                          ValidateIssuerSigningKey = true,
                          ValidIssuer = this.Configuration["Jwt:Issuer"],
                          ValidAudience = this.Configuration["Jwt:Issuer"],
                          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["Jwt:Key"]))
                      };
                  });*/

            DependencyInjection.AddApplication(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(errorApp => errorApp.Run(async context => await this.ErrorHandling(context)));

            app.UseCors("CorsPolicy");
            app.UseMiddleware(typeof(Common.Behaviours.ExceptionHandlerMiddleware));

            app.UseOpenApi();

            app.UseSwaggerUi3();

            app.UseHttpsRedirection();

            app.UseResponseCompression();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

           /* app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Media")),
                RequestPath = new PathString("/Media"),
            });*/
        }

        private async Task<HttpContext> ErrorHandling(HttpContext context)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "plain/text";

            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

            Logging.LogException(exceptionHandlerPathFeature?.Error);

            await context.Response.WriteAsync("An error occurred.");

            return context;
        }
    }
}
