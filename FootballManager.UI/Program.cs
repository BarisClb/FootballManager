using FootballManager.Application;
using FootballManager.Application.Configurations;
using FootballManager.Infrastructure;
using FootballManager.Persistence;
using FootballManager.UI.Configurations;
using FootballManager.UI.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, true)
                     .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, true);
builder.Services.AddControllersWithViews();
builder.Services.InjectSwaggerGen();
builder.Host.UseSerilog(ElasticsearchRegistration.Configure);
builder.Services.ApplicationServiceRegistration(builder.Configuration);
builder.Services.InfrastructureServiceRegistration();
builder.Services.PersistenceServiceRegistration(builder.Configuration);
builder.Services.HangfireServiceRegistration(builder.Configuration);

var app = builder.Build();
app.InjectSwaggerUI();
app.UseMiddleware<ErrorHandlerMiddleware>();
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.InitializeMsSqlDb();
app.HangfireDashboardRegistration(builder);
builder.Services.HangfireJobRegistration(builder.Configuration);

app.Run();
