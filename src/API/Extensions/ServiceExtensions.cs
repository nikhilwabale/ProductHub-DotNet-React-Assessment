using System.Text;
using Application.Interfaces;
using Asp.Versioning;
using Application.Mapping;
using Application.Services;
using Application.Validators;
using FluentValidation;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAssessmentServices(this IServiceCollection s, IConfiguration c)
    {
        s.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(c.GetConnectionString("DefaultConnection")));
        s.AddScoped<IUnitOfWork, UnitOfWork>();
        s.AddScoped<IProductService, ProductService>();
        s.AddScoped<IAuthService, AuthService>();
        s.AddScoped<ITokenService, JwtTokenService>();
        s.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
        s.AddAutoMapper(typeof(MappingProfile));
        s.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
        s.AddControllers();
        s.AddResponseCompression();

        s.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
            })
            .AddMvc()
            .AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });

        var key = Encoding.UTF8.GetBytes(c["Jwt:Key"]!);

        s.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = c["Jwt:Issuer"],
                ValidAudience = c["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            });

        s.AddAuthorization();
        s.AddCors(o => o.AddPolicy("Frontend", p => p
            .WithOrigins(c.GetSection("AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod()));

        s.AddEndpointsApiExplorer();
        s.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CRN Product Assessment API",
                Version = "v1"
            });

            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            o.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = Array.Empty<string>()
            });
        });

        return s;
    }
}
