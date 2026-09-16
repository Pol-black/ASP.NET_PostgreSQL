using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Role
{
    public class EditModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public EditModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.Role Role { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role =  await _context.Roles.FirstOrDefaultAsync(m => m.IdRole == id);
            if (role == null)
            {
                return NotFound();
            }
            Role = role;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Role).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(Role.IdRole))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.IdRole == id);
        }
    }
}
