using ExchangeRateProvider.Admin.Commons;
using ExchangeRateProvider.Infrastructure.Sql.Commons;
using Hangfire;
using HangfireBasicAuthenticationFilter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
await builder.ConfigureVaultServerAsync();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets()
   .RequireAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.UseInfrastructure();


app.UseHangfireDashboard("/exrp-workers", new DashboardOptions
{
    DashboardTitle = "Bacart background jobs and background workers",
    Authorization =
                [
                    new HangfireCustomBasicAuthenticationFilter
                    {
                        User = app.Configuration["hangFireUi:UserName"],
                        Pass = app.Configuration["hangFireUi:Password"]
                    }
                ]
});

await app.RunAsync();
