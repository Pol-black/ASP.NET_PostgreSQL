using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Purchase;
public class IndexModel : PageModel
{
    private readonly ArtMarketDbContext _context;

    public IndexModel(ArtMarketDbContext context)
    {
        _context = context;
    }

    // Параметры формы
    public string SearchString { get; set; } = "";
    public string MethodFilter { get; set; } = "";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public IList<Models.Purchase> Purchase { get; set; } = default!;

    public async Task OnGetAsync(string search, string method, DateOnly? start, DateOnly? end)
    {
        SearchString = search ?? "";
        MethodFilter = method ?? "";
        StartDate = start;
        EndDate = end;

        var query = _context.Purchases
            .Include(p => p.IdBuyerNavigation)
            .Include(p => p.IdSellerNavigation)
            .AsQueryable();

        // Поиск по номеру заказа
        if (!string.IsNullOrEmpty(SearchString))
        {
            query = query.Where(p => p.NumberPurchase.Contains(SearchString));
        }

        // Фильтр по способу доставки
        if (!string.IsNullOrEmpty(MethodFilter))
        {
            query = query.Where(p => p.MethodDelivery == MethodFilter);
        }

        // Фильтр по дате от
        if (StartDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= StartDate.Value);
        }

        // Фильтр по дате до
        if (EndDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= EndDate.Value);
        }

        Purchase = await query.ToListAsync();
    }
}