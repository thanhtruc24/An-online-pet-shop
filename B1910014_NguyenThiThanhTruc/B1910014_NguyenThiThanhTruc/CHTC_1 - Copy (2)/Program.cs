using AspNetCoreHero.ToastNotification;
using CHTC_1.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using System.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using CHTC_1.Mail;
using CHTC_1.Avatar;
using CHTC_1.Notification;

var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
           .SetBasePath(builder.Environment.ContentRootPath)
           .AddJsonFile("appsettings.json")
           .Build();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<Chtc8Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("constring")));

builder.Services.AddNotyf(config => { config.DurationInSeconds = 10; config.IsDismissable = true; config.Position = NotyfPosition.TopRight; });
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<EmailService>();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll",
//        builder =>
//        {
//            builder.AllowAnyHeader()
//                   .AllowAnyMethod()
//                   .SetIsOriginAllowed((host) => true)
//                   .AllowCredentials();
//        });
//});
//var VNPayConfig = builder.Configuration.GetSection("VNPayConfig").Get<VNPayConfig>();
//builder.Services.AddSingleton(VNPayConfig);
//builder.Services.AddScoped<IVnPayService,VnPayService>();
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromSeconds(10);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});



builder.Services.AddSession();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.LoginPath = new PathString("/Admin/AdminAccount/Login");
            options.AccessDeniedPath = new PathString("/auth/denied");
        })
    .AddGoogle("Google",options =>
    {
        options.ClientId = "756197428100-mqff306pc6qbauclb9i25ohalvcu7sn1.apps.googleusercontent.com";
        options.ClientSecret = "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";
    })
    .AddFacebook("Facebook",options =>
    {
        options.AppId = "837622087800439";
        options.AppSecret = "373e6a4a8d80d897cd58af3466294670";
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseCors();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();


app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotificationHub>("/notificationHub");
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=AdminAccount}/{action=Login}/{id?}"
    );
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
