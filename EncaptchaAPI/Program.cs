using EncaptchaAPI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(builder.Configuration.GetSection("Prices").Get<PricesSettings>());

var config = builder.Configuration.GetSection("Authorization").Get<AuthorizationSettings>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = config?.Issures,
        ValidateAudience = true,
        ValidAudience = config?.Audience,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config?.Key)),
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();

string connection = builder.Configuration.GetConnectionString("UserDb");
builder.Services.AddDbContext<UserContext>(options => options.UseSqlServer(connection));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


public class AuthorizationSettings
{
    public string Issures { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public string ExpiresHours { get; set; }
}

public class PricesSettings
{
    public string Bonus { get; set; }
    public string Captcha { get; set; }
}

