using EmpressOfLight.Data;
using EmpressOfLight.Models;
using EmpressOfLight.Helpers;
using EmpressOfLight.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<RSSFeedService>();
builder.Services.AddScoped<OrderService>();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<EmpressOfLightUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddSingleton<TwilioService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole",
         policy => policy.RequireRole("Admin"));
});
builder.Services.AddSingleton<IVnPayService, VnPayService>();

builder.Services.AddSingleton(x => new PaypalClient(
    builder.Configuration["PaypalOptions:AppId"],
    builder.Configuration["PaypalOptions:AppSecret"],
    builder.Configuration["PaypalOptions:Mode"]
    ));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllerRoute(name: "profile",
                pattern: "profile",
                defaults: new { controller = "Profile", action = "Index" });
app.MapControllerRoute(name: "product",
				pattern: "product/{productid}/{productname}",
				defaults: new { controller = "Product", action = "Index" });
app.MapControllerRoute(name: "productmanage",
                pattern: "ManageProduct",
                defaults: new { controller = "Product", action = "Manage" });
app.MapControllerRoute(name: "productedit",
                pattern: "ManageProduct/{id}",
                defaults: new { controller = "Product", action = "Edit" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "emailSub",
    pattern: "emailsub/{action=ExportEmails}",
    defaults: new { controller = "EmailSub" });
app.MapRazorPages();

app.Run();
