using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rafiq.Data_Access;
using Rafiq.Models;
using Rafiq.Repositories;
using Rafiq.Repositories.IRepositories;
using Rafiq.Services;
using Rafiq.Utitlies;
using Rafiq.Utitlies.DBinitializer;
using Scalar.AspNetCore;
using Stripe;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString =
     builder.Configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Connection string"
         + "'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(option =>
{
    option.User.RequireUniqueEmail = true;
    option.Password.RequiredLength = 8;
    option.Password.RequireNonAlphanumeric = false;
    option.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();



builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<ITokenService, Rafiq.Services.TokenService>();
builder.Services.AddHttpClient<IDeepSeekService, DeepSeekService>();
builder.Services.AddScoped<IDBinitializer, DBinitializer>();

builder.Services.AddScoped<IRepository<ApplicationUserOTP>, Repository<ApplicationUserOTP>>();
builder.Services.AddScoped<IRepository<Medicine>, Repository<Medicine>>();
builder.Services.AddScoped<IRepository<Rafiq.Models.Product>, Repository<Rafiq.Models.Product>>();
builder.Services.AddScoped<IRepository<Advertisement>, Repository<Advertisement>>();
builder.Services.AddScoped<IRepository<DidYouKnow>, Repository<DidYouKnow>>();
builder.Services.AddScoped<IRepository<ImportantQuestions>, Repository<ImportantQuestions>>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IRepository<Category>, Repository<Category>>();
builder.Services.AddScoped<IRepository<ActiveIngredient>, Repository<ActiveIngredient>>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
builder.Services.AddScoped<ICosmeticesRepository, CosmeticesRepository>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<INotificationService, NotificationService>();
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

var jwtIssuer = "https://rafiq1.runasp.net";
var jwtAudience = "https://rafiq1.runasp.net";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidAlgorithms = new[]
            {
                SecurityAlgorithms.HmacSha256
            }
        };
    });





StripeConfiguration.ApiKey = builder.Configuration ["Stripe:SecretKey"];



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "https://maroon-oyster-870575.hostingersite.com", "https://rafiqpharma.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});



var app = builder.Build();

var scope = app.Services.CreateScope();
var service = scope.ServiceProvider.GetService<IDBinitializer>();
service!.initialize();

app.UseCors("AllowAngular");

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
