using MyRazorPage;
using MyRazorPage.Models;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

//Bổ sung 1 service làm việc với các pages vào container Kestrel
builder.Services.AddRazorPages();

//add DBcontext
builder.Services.AddDbContext<PRN221_DBContext>();
builder.Services.AddSignalR();
//add session
builder.Services.AddSession(opt => opt.IdleTimeout = TimeSpan.FromMinutes(2));


builder.Services.AddSession();
//add cookie



builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Signin";
        options.AccessDeniedPath = "/404Page";
    });

var app = builder.Build();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<HubServer>("/hub");
app.UseSession();
app.Run();