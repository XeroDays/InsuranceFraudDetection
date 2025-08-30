using InsuranceFraudDetection.BackgroundServices;
using InsuranceFraudDetection.Infrastructure.Data;
using InsuranceFraudDetection.Infrastructure.DependencyInjection;
using InsuranceFraudDetection.Infrastructure.Logging;
using InsuranceFraudDetection.Infrastructure.Middleware;
using InsuranceFraudDetection.Infrastructure.SignalR.Hubs;
using InsuranceFraudDetection.Presentation.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomLogger(builder.Configuration);
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new PresentationLocationExpander());
});
builder.Services.AddSingleton<IBackgroundTaskQueue>(provider => new BackgroundTaskQueue(capacity: builder.Configuration.GetValue<int>("EmailSettings:MaxQueueDepth"), logger: provider.GetRequiredService<ILogger<BackgroundTaskQueue>>()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
    try
    {
        context.Database.Migrate(); 
        
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while applying database migrations: {ex.Message}");
    }
}

if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseRouting(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();
app.MapHub<FraudDetectionHub>("/fraudDetectionHub");

app.Run();
