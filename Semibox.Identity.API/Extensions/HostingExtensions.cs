using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Semibox.Identity.API.Services;
using Semibox.Identity.Domain.Entities;
using Semibox.Identity.Infrastructure.Middlewares;
using Semibox.Identity.Persistence;
using Semibox.Identity.Persistence.DataContexts;
using Serilog;
using System.Text;

namespace Semibox.Identity.API.Extensions
{
    public static class HostingExtensions
    {
        public static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder)
        {
            #region Controller
            builder.Services.AddControllers();
            #endregion

            #region Swagger
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options => { 
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Semibox Identity",
                    Version = "v1",
                    Description = "Identity for Semibox",
                    Contact = new OpenApiContact
                    {
                        Url = new Uri("https://google.com"),
                        Name = "Semibox",

                    }
                });
            var securitySchema = new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", securitySchema);

            var securityRequirement = new OpenApiSecurityRequirement
                {
                { securitySchema, new[] { "Bearer" } }
                };

            options.AddSecurityRequirement(securityRequirement);
        });
            #endregion

            #region DataContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services
                .AddDbContext<DataContext>(options => options.UseNpgsql(connectionString));
            #endregion

            #region Identity
            builder.Services
                .AddIdentityCore<AppUser>()
                .AddEntityFrameworkStores<DataContext>()
                .AddDefaultTokenProviders();
            #endregion

            #region Authentication
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["TokenKey"]));

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
            #endregion

            #region Services
            builder.Services
                .AddScoped<TokenService>();
            #endregion

            return builder;
        }

        public static async Task<WebApplication> ConfigurePipelineAsync(this WebApplication app)
        {
            // Configure the HTTP request pipeline.

            #region Serilog
            app.UseSerilogRequestLogging();
            #endregion

            #region Middlewares
            app.UseMiddleware<ExceptionMiddleware>();
            #endregion

            #region Swagger
            app.UseSwagger();
            app.UseSwaggerUI();
            #endregion

            #region Development
            if (app.Environment.IsDevelopment())
            {
                
            }
            #endregion

            app.UseHttpsRedirection();

            #region Authentication
            app.UseAuthentication();
            #endregion

            #region Authorization
            app.UseAuthorization();
            #endregion

            #region Seed
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<DataContext>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            await context.Database.MigrateAsync();
            await Seeder.SeedAsync(userManager);

            #endregion

            app.MapControllers();

            return app;
        }
    }
}
