using InsuranceFraudDetection.Infrastructure.DependencyInjection;
using InsuranceFraudDetection.Presentation.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using InsuranceFraudDetection.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using InsuranceFraudDetection.Infrastructure.SignalR.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new PresentationLocationExpander());
});

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
