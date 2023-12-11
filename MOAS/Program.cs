using MOAS.Interfaces;
using MOAS.Models;
using MOAS.Repositories;
using DevExpress.AspNetCore.Reporting;
using DevExpress.AspNetCore;
using GridMvc;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/LogOn";
                options.AccessDeniedPath = "/Account/NoPermission";
                options.Cookie.Name = "CoreTemplate";

            });
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<MOASContext>();
builder.Services.AddTransient<ISetupRepository,SetupRepository>();

builder.Services.AddGridMvc();

//builder.Services.AddDevExpressControls();
builder.Services.AddMvc();
//builder.Services.ConfigureReportingServices(configurator => {
//    configurator.ConfigureWebDocumentViewer(viewerConfigurator => {
//        viewerConfigurator.UseCachedReportSourceBuilder();
//    });
//});


var app = builder.Build();

//app.UseDevExpressControls();
//System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
