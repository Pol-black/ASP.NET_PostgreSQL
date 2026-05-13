using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace RazorPagesMovie.Pages.Product
{
    public class DetailsModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public DetailsModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        public Models.Product Product { get; set; } = default!;
        public string ReturnUrl { get; set; } = default!;


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {   return NotFound();
            }

            // задаём адрес ссылки для кнопки возврата на предыдыщую страницу
            // если есть адрес. Или на Индекс
            ReturnUrl = Request.Headers["Referer"].ToString() ?? "/Index";

            var product = await _context.Products
                .Include(p => p.IdSellerNavigation)
                .Include(p => p.IdIndivBuyerNavigation)
                .Include(p => p.ConnectProductMaterials)
                    .ThenInclude(cpm => cpm.IdMaterialNavigation)
                .FirstOrDefaultAsync(m => m.IdProduct == id);

            if (product == null)
            {
                return NotFound();
            }

            Product = product;
            return Page();
        }
    }
}