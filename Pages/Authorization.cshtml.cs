using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
//using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using RazorPagesMovie.Models;
//using System.Data;
using System.Security.Claims;

namespace RazorPagesMovie.Pages
{
    public class AuthorizationModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public AuthorizationModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string LoginInput { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }



        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(LoginInput) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите имя/email и пароль.";
                return Page();
            }

            // 🔍 Ищем пользователя по AccountName ИЛИ Email
            var user = await _context.Accounts
                .Include(a => a.IdRoleNavigation)
                .FirstOrDefaultAsync(a =>
                    (a.AccountName == LoginInput || a.Email == LoginInput) &&
                    a.Password == Password);

            // 💡 НАЧАЛО: ЛОГИРОВАНИЕ ДЛЯ ОТЛАДКИ
            if (user == null) {Console.WriteLine($"❌ ПОЛЬЗОВАТЕЛЬ НЕ НАЙДЕН! LoginInput='{LoginInput}', Password='{Password}'"); }
            else  {
                string roleName = user.IdRoleNavigation?.RoleName ?? "null";
                Console.WriteLine($"✅ НАЙДЕН ПОЛЬЗОВАТЕЛЬ! Имя='{user.AccountName}', Email='{user.Email}', Роль='{roleName}'");
            }
            // 💡 КОНЕЦ: ЛОГИРОВАНИЕ

            if (user != null && user.IdRoleNavigation != null)
            {
                string accountName = user.AccountName ?? "null";
                string roleName = user.IdRoleNavigation?.RoleName ?? "null";
                string email = user.Email;
                int IdAccount = user.IdAccount;

                // сохраняем логин, емайл, роль пользователя в Claim и куку
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name,  accountName),
                    new(ClaimTypes.Email, email),
                    new(ClaimTypes.Role,  roleName),
                    new("idaccount",      IdAccount.ToString())
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // запись в куки зашифрованной строки с логином, емайл, ролью
                // Настройка свойств аутентификации
                var authProperties = new AuthenticationProperties
                {  // Указываем, что кука будет сохранена и не удалится при закрытии браузера
                    IsPersistent = true,
                    // Устанавливаем срок действия куки 15 минут
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(15)
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);


                Console.WriteLine($"➡️ Перенаправление по роли: {user.IdRoleNavigation.RoleName}");

                // Перенаправление по роли
                return user.IdRoleNavigation.RoleName switch
                {
                    "admin" => RedirectToPage("/Admin"),
                    "buyer" => RedirectToPage("/Index"),
                    "seller" => RedirectToPage("/Seller/Index"),
                    _ => RedirectToPage("/Authorization")
                };
            }

            ErrorMessage = "Неверное имя/email или пароль.";
            return Page();
        }
    }
}