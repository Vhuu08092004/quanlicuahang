using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quanlicuahang.Data;
using Quanlicuahang.Exception;
using Quanlicuahang.Repositories;
using Quanlicuahang.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------------- CORS CONFIG ----------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000") // React app
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ---------------------- JWT CONFIG ----------------------
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
// ---------------------- DATABASE CONFIG ----------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ---------------------- APP SERVICES ----------------------
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddApplicationServices();

builder.Services.AddControllers();

// ---------------------- SWAGGER ----------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------------- BUILD APP ----------------------
var app = builder.Build();

// ---------------------- MIDDLEWARE PIPELINE ----------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} else {
    app.UseHttpsRedirection();
}


// ✅ Enable CORS before authentication & authorization
app.UseCors("AllowReactApp");

// ✅ Enable Authentication middleware
app.UseAuthentication();

// ✅ Enable Authorization middleware
app.UseAuthorization();

app.MapControllers();

app.Run();
