using Application.Features;
using Application.Service;
using Application.Services;
using Core.Interfaces;
using Infrastructure.Authentication;
using Infrastructure.Persistence;
using Infrastructure.RealTimeUpdate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddFeatureManagement();

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));


// Register DbContext with SQL Server
builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
      IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    options.Events = new JwtBearerEvents
    {
      OnAuthenticationFailed = context =>
      {
        Console.WriteLine("Authentication failed: " + context.Exception.Message);
        return Task.CompletedTask;
      },
      OnTokenValidated = context =>
      {
        Console.WriteLine("Token validated");
        return Task.CompletedTask;
      },
    };
  }
);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("QuizApp"))
    .WithTracing(tracerProviderBuilder =>
    {
      tracerProviderBuilder
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddSource("QuizApp.SubmitAnswer")
          .AddJaegerExporter(options =>
          {
            options.AgentHost = "localhost";
            options.AgentPort = 6831;
          });
      //.AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
      metrics
      //.AddAspNetCoreInstrumentation()
      .AddMeter("QuizApp")
      //.AddConsoleExporter()
      .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
      {
        exporterOptions.Endpoint = new Uri("http://localhost:9090/api/v1/otlp/v1/metrics");
        exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
        metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
      })
      ;
    });

builder.Services.AddSingleton(new ActivitySource("QuizApp.SubmitAnswer"));
builder.Services.AddSingleton(new Meter("QuizApp"));

builder.Services.AddScoped<IAuthenticate, Authenticate>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<QuizService>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizSessionService, RedisSessionService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<RequestTimeOutFilter>();
builder.Services.AddScoped<AntiCheatHeaderFilter>();
builder.Services.AddScoped<SubmissionService>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IRealTimeLeaderboardNotifier, RealTimeLeaderboardNotifier>();
builder.Services.AddScoped<StreakBonus>();

builder.Services.AddAuthorization();

builder.Services.AddSignalR(options =>
{
  options.EnableDetailedErrors = true;
});

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var app = builder.Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("../Infrastructure/Logger/app.log", shared: true)
    .WriteTo.Map(
        "QuizId",
        (quizId, wt) => wt.File($"../Infrastructure/Logger/quiz-{quizId}.log", shared: true)
    )
    .CreateLogger();

app.MapHub<LeaderBoardHub>("/leaderboard");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();
Log.Information("Application Started");