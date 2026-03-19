using AILifeAnalytics.Application.Services;
using AILifeAnalytics.Domain.Interfaces;
using AILifeAnalytics.Infrastructure.AI;
using AILifeAnalytics.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "AI Life Analytics API",
        Version = "v1",
        Description = "Behavioral analytics platform with AI-powered insights"
    });
});

builder.Services.AddSingleton<IActivityRepository, ActivityRepository>();
builder.Services.AddSingleton<IInsightRepository, InsightRepository>();
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddHttpClient("OpenAI");
builder.Services.AddScoped<IAIService, OpenAIService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Logging.AddConsole();

var app = builder.Build();

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
app.UseRouting();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
