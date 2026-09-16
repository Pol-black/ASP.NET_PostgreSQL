using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Role
{
    public class CreateModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public CreateModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Models.Role Role { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Roles.Add(Role);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
