using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesMovie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorPagesMovie.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public RegisterModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string AccountName { get; set; } = string.Empty;

        [BindProperty]
        public string Address { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Phone { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public int IdRole { get; set; }

        public IEnumerable<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // Загружаем список ролей из базы данных
            RoleList = await _context.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.IdRole.ToString(),
                    Text = r.RoleName
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Перезагружаем список ролей, если валидация не прошла
                RoleList = await _context.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.IdRole.ToString(),
                        Text = r.RoleName
                    })
                    .ToListAsync();
                return Page();
            }

            // Проверка: email должен быть уникальным
            if (await _context.Accounts.AnyAsync(a => a.Email == Email))
            {
                ErrorMessage = "Пользователь с таким email уже существует.";
                RoleList = await _context.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.IdRole.ToString(),
                        Text = r.RoleName
                    })
                    .ToListAsync();
                return Page();
            }

            // Создание нового аккаунта
            var newAccount = new Models.Account
            {
                AccountName = AccountName,
                Address = Address,
                Email = Email,
                Phone = Phone,
                Password = Password, // ⚠️ Только для учебного проекта!
                IdRole = IdRole
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            // После успешной регистрации — перенаправляем на страницу входа
            return RedirectToPage("/Authorization/Authorization");
        }
    }
}