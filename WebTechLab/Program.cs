using Microsoft.EntityFrameworkCore;
using WebTechLab.Models;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

string connectionString;


if (builder.Environment.IsEnvironment("Docker"))
{

    connectionString = builder.Configuration.GetConnectionString("DockerConnection");
}
else
{

    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllersWithViews();

var app = builder.Build();

var invariantCulture = CultureInfo.InvariantCulture;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(invariantCulture),
    SupportedCultures = new List<CultureInfo> { invariantCulture },
    SupportedUICultures = new List<CultureInfo> { invariantCulture }
});
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
