using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Включим поддержку Pages/ с файлами *.cshtml + *.cshtml.cs 
builder.Services.AddRazorPages();


// подключаем БД с логированием
builder.Services.AddDbContext<RazorPagesMovie.Models.ArtMarketDbContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("Server=localhost;Database=art_market_db;Username=postgres;Password=1234; Persist Security Info=True"));
    // отладка - вывод логов в консоль
    options.LogTo(Console.WriteLine, LogLevel.Information); // Печатать логи в консоль
    options.EnableSensitiveDataLogging(); // Чтобы видеть значения параметров (имена, пароли, значения переменных в запросах)
});

// авторизация сделана по примеру https://metanit.com/sharp/aspnet6/13.7.php
// включаем сервис аутонтификации на основе куки
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {   options.LoginPath = "/Authorization";
        options.AccessDeniedPath = "/accessdenied";
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

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
