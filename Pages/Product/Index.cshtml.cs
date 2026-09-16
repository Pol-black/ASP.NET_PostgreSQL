using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Product;

public class IndexModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public IndexModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    // Параметры формы
    public string SearchString { get; set; } = "";
    public string StatusFilter { get; set; } = "";

    public IList<Models.Product> Product { get; set; } = default!;

    public async Task OnGetAsync(string search, string status)
    {
        SearchString = search ?? "";
        StatusFilter = status ?? "";

        var query = _context.Products
            .Include(p => p.IdIndivBuyerNavigation)
            .Include(p => p.IdSellerNavigation)
            .AsQueryable();

        // Поиск по названию или типу искусства
        if (!string.IsNullOrEmpty(SearchString))
        {
            query = query.Where(p =>
                p.Name.Contains(SearchString) ||
                p.TypeArt.Contains(SearchString));
        }

        // Фильтр по статусу
        if (!string.IsNullOrEmpty(StatusFilter))
        {
            query = query.Where(p => p.Status == StatusFilter);
        }

        Product = await query.ToListAsync();
    }
}