using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Data.SqlClient;
using System.Text;
using vehicleAccounting.Api.data;
using vehicleAccounting.Api.data.repositories;
using vehicleAccounting.Api.data.services;
using vehicleAccounting.Data.interfaces;
using vehicleAccounting.Data.models;
using vehicleAccounting.Data.utils;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddCors();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
          options.RequireHttpsMetadata = false;
          options.SaveToken = true;
          options.TokenValidationParameters = new TokenValidationParameters
          {
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateLifetime = true,
              ValidateIssuerSigningKey = true,
              ValidIssuer = builder.Configuration[ConstProgram.VALID_ISSUER],
              ValidAudience = builder.Configuration[ConstProgram.VALID_AUDIENCE],
              IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration[ConstProgram.ISSUER_SIGNING_KEY]))
          };
      });

    var jwtConfig = builder.Configuration
        .GetSection(ConstProgram.SECTION_JWT)
        .Get<JWTConfiguration>();
    builder.Services.AddSingleton(jwtConfig);

    var connectionString = builder.Configuration
        .GetSection(ConstProgram.SECTION_CONNECTION_STRING)
        .Get<ConnectionStringConfiguration>();
    builder.Services.AddSingleton(connectionString);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(sw =>
    sw.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "dot6JWTAuthentication",
            Version = "1.0"
        }
    ));

    builder.Services.AddSwaggerGen(sw =>
        sw.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Insert JWT Token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        })
    );

    builder.Services.AddSwaggerGen(sw =>
        sw.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },

                    Array.Empty<string>()
                }
            }
    ));

    builder.Services.AddScoped<IJwt, JwtService>();
    builder.Services.AddScoped<ICar, CarRepository>();
    builder.Services.AddScoped<IAuth, AuthRepository>();
}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseCors(builder => builder.
        WithOrigins(ConstProgram.WITH_ORIGINS)
       .AllowAnyHeader()
       .AllowAnyMethod());

    app.MapControllers();

    //Adding objects to the database
    using (var scope = app.Services.CreateAsyncScope())
    {
        using var connection = new SqlConnection(builder.Configuration.GetConnectionString("ConnectionString"));
        await DBObjects.Initial(connection);
    }

    app.Run();
}