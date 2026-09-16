using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorPagesMovie.Pages.Account
{
    public class CreateModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public CreateModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        // Свойство для передачи списка ролей в представление
        public SelectList RoleList { get; set; } = default!;

        [BindProperty]
        public Models.Account Account { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var roles = await _context.Roles.ToListAsync();
            if (!roles.Any())
            {
                ModelState.AddModelError("", "Нет доступных ролей. Пожалуйста, добавьте роли в базу данных.");
            }
            RoleList = new SelectList(roles, "IdRole", "RoleName");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Перезаполняем список при ошибке валидации
                var roles = await _context.Roles.ToListAsync();
                RoleList = new SelectList(roles, "IdRole", "RoleName");
                return Page();
            }

            try
            {
                _context.Accounts.Add(Account);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ошибка при сохранении: " + ex.Message);
                var roles = await _context.Roles.ToListAsync();
                RoleList = new SelectList(roles, "IdRole", "RoleName");
                return Page();
            }
        }
    }
}