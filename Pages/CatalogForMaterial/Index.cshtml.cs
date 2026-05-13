using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.CatalogForMaterial;

public class IndexModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public IndexModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    // Параметры формы
    public string SearchString { get; set; } = "";
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public IList<Models.CatalogForMaterial> CatalogForMaterial { get; set; } = default!;

    public async Task OnGetAsync(string search, decimal? minPrice, decimal? maxPrice)
    {
        SearchString = search ?? "";
        MinPrice = minPrice;
        MaxPrice = maxPrice;

        var query = _context.CatalogForMaterials.AsQueryable();

        // Поиск по названию
        if (!string.IsNullOrEmpty(SearchString))
        {
            query = query.Where(m => m.MaterialName.Contains(SearchString));
        }

        // Фильтр по минимальной цене
        if (MinPrice.HasValue && MinPrice > 0)
        {
            query = query.Where(m => m.CostPerUnit >= MinPrice.Value);
        }

        // Фильтр по максимальной цене
        if (MaxPrice.HasValue && MaxPrice > 0)
        {
            query = query.Where(m => m.CostPerUnit <= MaxPrice.Value);
        }

        CatalogForMaterial = await query.ToListAsync();
    }
}
