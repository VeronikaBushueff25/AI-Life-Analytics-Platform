using AILifeAnalytics.Application.Services;
using AILifeAnalytics.Domain.Interfaces;
using AILifeAnalytics.Infrastructure.AI;
using AILifeAnalytics.Infrastructure.Persistence;
using AILifeAnalytics.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AI Life Analytics API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new()
    {
        [new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } }] = []
    });
});
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IActivityRepository, EfActivityRepository>();
builder.Services.AddScoped<IInsightRepository, EfInsightRepository>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IUserSettingsRepository, EfUserSettingsRepository>();

builder.Services.AddSingleton<ISettingsRepository, SettingsRepository>();

builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddHttpClient("OpenAI");
builder.Services.AddHttpClient("DeepSeek");
builder.Services.AddHttpClient("HuggingFace");
builder.Services.AddHttpClient("GoogleAI");
builder.Services.AddHttpClient("Ollama");

builder.Services.AddScoped<IAIProvider, OpenAIProvider>();
builder.Services.AddScoped<IAIProvider, DeepSeekProvider>();
builder.Services.AddScoped<IAIProvider, HuggingFaceProvider>();
builder.Services.AddScoped<IAIProvider, GoogleAIProvider>();
builder.Services.AddScoped<IAIService, AIProviderFactory>();

builder.Services.AddScoped<IPersonalityProfileRepository, EfPersonalityProfileRepository>();
builder.Services.AddScoped<PersonalityProfileService>();

builder.Services.AddScoped<ICbtRepository, EfCbtRepository>();
builder.Services.AddScoped<CbtService>();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Logging.AddConsole();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();  
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Life Analytics v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();   
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();