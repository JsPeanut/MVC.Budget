using Microsoft.EntityFrameworkCore;
using MVC.Budget.JsPeanut.Data;
using MVC.Budget.JsPeanut.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

CultureInfo culture = new CultureInfo("en-US");
culture.NumberFormat.NumberDecimalSeparator = ".";
culture.NumberFormat.NumberGroupSeparator = ",";
CultureInfo.DefaultThreadCurrentCulture = culture;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<CategoryService, CategoryService>();
builder.Services.AddTransient<TransactionService, TransactionService>();
builder.Services.AddTransient<JsonFileCurrencyService, JsonFileCurrencyService>();
builder.Services.AddTransient<CurrencyConverterService, CurrencyConverterService>();
var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

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
    name: "Categories",
    pattern: "{controller=Categories}/{action=Index}/{id?}");

app.Run();
