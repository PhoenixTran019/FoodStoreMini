using FoodStore.Application.Auth;
using FoodStore.Application.Interface.Admin;
using FoodStore.Application.Interface.Auth;
using FoodStore.Application.Interface.Menu;
using FoodStore.Application.Interface.Orders;
using FoodStore.Domain.Data;
using FoodStore.Infrastructure.Services.Admin;
using FoodStore.Infrastructure.Services.Auths;
using FoodStore.Infrastructure.Services.Menu;
using FoodStore.Infrastructure.Services.Orders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.IdentityModel.Tokens.Jwt;


JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
var builder = WebApplication.CreateBuilder(args);

//Cấu hình DBContext với SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNgrok", p => p
        .SetIsOriginAllowed(origin =>
            origin == "https://aryan-hypaesthesic-answerably.ngrok-free.dev"
            || origin.StartsWith("http://localhost:")
            || origin.StartsWith("https://localhost:")
            || origin.StartsWith("http://127.0.0.1:")
            || origin.StartsWith("https://127.0.0.1:")
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddHttpContextAccessor();

//Đăng ký Swagger
builder.Services.AddEndpointsApiExplorer();
// --- 3. ĐĂNG KÝ SWAGGER (Cấu hình có nút Authorize) ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "FoodStore API", Version = "v1" });

    // Cấu hình nút Authorize chuẩn 
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập: Bearer [token]"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            System.Array.Empty<string>()
        }
    });
});




// 1. Đăng ký Options (đọc từ appsettings)
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// 2. Đăng ký dịch vụ 
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAdminCustomerService, AdminCustomerService>();
builder.Services.AddScoped<ICusOrderService, CusOrderService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();


// 3. Cấu hình Authentication (Bắt buộc)
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

// Configure the HTTP request pipeline.

var app = builder.Build();

//Kích hoạt Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Tạo file swagger.json
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FoodStore API v1");
        options.RoutePrefix = "swagger"; // Đường dẫn truy cập sẽ là localhost:xxxx/swagger
    });
}

string uploadPath = Path.Combine(builder.Environment.ContentRootPath, "..", "FoodStore.Uploads", "uploads");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

app.UseRouting();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
