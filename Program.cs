using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Включим поддержку Pages/ с файлами *.cshtml + *.cshtml.cs 
builder.Services.AddRazorPages();

// подключаем БД
var connectionString = builder.Configuration.GetConnectionString("ArtMarketDb")
    ?? throw new InvalidOperationException("Не задана строка подключения 'ArtMarketDb'. Проверьте секреты пользователя.");

builder.Services.AddDbContext<RazorPagesMovie.Models.ArtMarketDbContext>(options =>
{
    options.UseNpgsql(connectionString);

    // Логи с значениями параметров — только при локальной разработке
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information);
        options.EnableSensitiveDataLogging();
    }
});

// авторизация сделана по примеру https://metanit.com/sharp/aspnet6/13.7.php
// включаем сервис аутонтификации на основе куки
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {   options.LoginPath = "/Authorization";
        options.AccessDeniedPath = "/accessdenied";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{   app.UseExceptionHandler("/Error");
    app.UseHsts();
}else{
    app.UseDeveloperExceptionPage();
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // включаем посредников аутентификации и авторизации
app.UseAuthorization();

app.MapStaticAssets(); //оптимизируем(сжимаем) статические файлы в wwwroot
app.MapRazorPages().WithStaticAssets(); // подключаем маршруты папки Pages/


app.Run();
