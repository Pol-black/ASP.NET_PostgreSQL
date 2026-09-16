using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Product
{
    public class DeleteModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public DeleteModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.Product Product { get; set; } = default!;
        [BindProperty]
        public string ReturnUrl { get; set; } = default!;
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ReturnUrl = Request.Headers["Referer"].ToString() ?? "/Index";

            var product = await _context.Products.FirstOrDefaultAsync(m => m.IdProduct == id);

            if (product is not null)
            {
                Product = product;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ConnectProductMaterials) // Включаем связанные материалы
                .FirstOrDefaultAsync(p => p.IdProduct == id);

            if (product != null)
            {
                // Сначала удаляем все связанные записи
                _context.ConnectProductMaterials.RemoveRange(product.ConnectProductMaterials);

                // Затем удаляем сам товар
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();
            }

            ReturnUrl = Request.Headers["Referer"].ToString() ?? "/Index";
            return Redirect(ReturnUrl);
        }
    }
}
