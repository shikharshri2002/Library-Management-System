using System.Text;
using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LibraryDatabase")));
builder.Services.AddCors(options => options.AddPolicy("Angular", policy =>
    policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"])
        .AllowAnyHeader()
        .AllowAnyMethod()));

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Configure Jwt:Key with a secret of at least 32 bytes.");
}
if (!builder.Environment.IsDevelopment() &&
    jwtKey == "development-only-change-this-key-before-deployment-please")
{
    throw new InvalidOperationException("Replace the development Jwt:Key before running outside Development.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    await db.Database.MigrateAsync();

    var adminEmail = app.Configuration["BootstrapAdmin:Email"]?.Trim().ToLowerInvariant();
    var adminPassword = app.Configuration["BootstrapAdmin:Password"];
    if (string.IsNullOrWhiteSpace(adminEmail) != string.IsNullOrWhiteSpace(adminPassword))
    {
        throw new InvalidOperationException(
            "Set both BootstrapAdmin:Email and BootstrapAdmin:Password to create the initial administrator.");
    }

    if (!string.IsNullOrWhiteSpace(adminEmail) && adminPassword is not null)
    {
        if (adminPassword.Length < 12)
            throw new InvalidOperationException("BootstrapAdmin:Password must contain at least 12 characters.");

        var admin = await db.Users.SingleOrDefaultAsync(user => user.Email == adminEmail);
        if (admin is null)
        {
            admin = new User
            {
                Name = "Library Administrator",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = Roles.Admin
            };
            admin.Member = new Member
            {
                Name = admin.Name,
                Email = admin.Email,
                User = admin
            };
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }
        else if (admin.Role != Roles.Admin)
        {
            throw new InvalidOperationException(
                "The configured bootstrap account already exists without the Admin role.");
        }
    }
}

app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
