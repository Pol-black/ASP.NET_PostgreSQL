using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RazorPagesMovie.Pages
{
    public class IndexViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TypeArt { get; set; }
        public decimal Price { get; set; }
        public int QuantityForSale { get; set; }
        public bool IsIndividual { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public IndexModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        public int? CurrentBuyerId { get; set; }

        public List<IndexViewModel> IndividualProducts { get; set; } = new List<IndexViewModel>();
        public List<IndexViewModel> CatalogProducts { get; set; } = new List<IndexViewModel>();

        public async Task OnGetAsync()
        {
            // 1. Получаем email текущего пользователя из claims (без изменения авторизации)
            var emailClaim = User.FindFirst(ClaimTypes.Email);
            string userEmail = emailClaim?.Value;

            // 2. Если пользователь авторизован, находим его ID по email
            if (!string.IsNullOrEmpty(userEmail))
            {
                var userAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Email == userEmail);

                if (userAccount != null)
                {
                    CurrentBuyerId = userAccount.IdAccount;
                }
            }

            // 3. Загружаем ВСЕ товары (без фильтрации по статусу для индивидуальных товаров)
            var allProducts = await _context.Products
                .Include(p => p.IdIndivBuyerNavigation)
                .ToListAsync();

            // 4. Для авторизованного пользователя ищем персональные товары
            if (CurrentBuyerId.HasValue)
            {
                // Ищем товары, созданные специально для этого пользователя
                // Включаем товары в статусе "reserved" (индивидуальные) и "available"
                IndividualProducts = allProducts
                .Where(p => p.IdIndivBuyer == CurrentBuyerId.Value && 
                        (p.Status == "reserved" || p.Status == "available"))
                .Select(p => new IndexViewModel
                {
                    Id = p.IdProduct,
                    Name = p.Name,
                    TypeArt = p.TypeArt,
                    Price = p.Price,
                    QuantityForSale = p.QuantityForSale,
                    IsIndividual = true
                })
                .ToList();
            }

            // 5. Показываем только общедоступные товары со статусом "available"
            CatalogProducts = allProducts
                .Where(p => p.IdIndivBuyer == null && p.Status == "available")
                .Select(p => new IndexViewModel
                {
                    Id = p.IdProduct,
                    Name = p.Name,
                    TypeArt = p.TypeArt,
                    Price = p.Price,
                    QuantityForSale = p.QuantityForSale,
                    IsIndividual = false
                })
                .ToList();
        }

        public async Task<IActionResult> OnPostOrderAsync(int productId)
        {
            // Проверяем авторизацию пользователя
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["ErrorMessage"] = "Для оформления заказа необходимо войти в систему.";
                return RedirectToPage("/Authorization");
            }

            // Получаем email текущего пользователя
            var emailClaim = User.FindFirst(ClaimTypes.Email);
            if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
            {
                TempData["ErrorMessage"] = "Не удалось определить ваш email. Пожалуйста, войдите в систему снова.";
                return RedirectToPage("/Authorization");
            }

            string userEmail = emailClaim.Value;

            // Находим пользователя по email
            var userAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == userEmail);

            if (userAccount == null)
            {
                TempData["ErrorMessage"] = "Пользователь с таким email не найден.";
                return RedirectToPage("/Authorization");
            }

            CurrentBuyerId = userAccount.IdAccount;

            // Находим товар для заказа
            var productToOrder = await _context.Products
                .FirstOrDefaultAsync(p => p.IdProduct == productId);

            if (productToOrder == null)
            {
                TempData["ErrorMessage"] = "Выбранный товар не найден.";
                return RedirectToPage();
            }

            // Проверяем, доступен ли товар для заказа
            bool isAvailableForOrder =
                (productToOrder.Status == "available") ||
                (productToOrder.IdIndivBuyer == CurrentBuyerId.Value &&
                 (productToOrder.Status == "reserved" || productToOrder.Status == "available"));

            if (!isAvailableForOrder)
            {
                TempData["ErrorMessage"] = "Этот товар сейчас недоступен для заказа.";
                return RedirectToPage();
            }

            // Проверяем, достаточно ли товара на складе (для общих товаров)
            if (productToOrder.IdIndivBuyer == null && productToOrder.QuantityForSale < 1)
            {
                TempData["ErrorMessage"] = "К сожалению, этого товара нет в наличии.";
                return RedirectToPage();
            }

            try
            {
                // 1. Создание нового заказа (Purchase)
                var newPurchase = new Models.Purchase
                {
                    NumberPurchase = "ORD" + System.DateTime.Now.Ticks.ToString(),
                    IdSeller = productToOrder.IdSeller,
                    IdBuyer = CurrentBuyerId.Value,
                    AddressDeparture = "Адрес продавца будет указан при подтверждении заказа",
                    AddressReceiving = "Ваш адрес доставки будет указан при оформлении заказа",
                    MethodDelivery = "courier",
                    CreatedAt = DateOnly.FromDateTime(System.DateTime.Now)
                };

                await _context.Purchases.AddAsync(newPurchase);
                await _context.SaveChangesAsync();

                // 2. Создание позиции заказа (ItemPurchase)
                var newItemPurchase = new Models.ItemPurchase
                {
                    IdPurchase = newPurchase.IdPurchase,
                    IdProduct = productToOrder.IdProduct,
                    QuantityInPurchase = 1,
                    PriceInPurchase = productToOrder.Price,
                    StatusItem = "pending"
                };

                await _context.ItemPurchases.AddAsync(newItemPurchase);

                // 3. Обновление статуса товара
                if (productToOrder.IdIndivBuyer.HasValue)
                {
                    productToOrder.Status = "reserved";
                }
                else
                {
                    productToOrder.QuantityForSale -= 1;
                    if (productToOrder.QuantityForSale <= 0)
                    {
                        productToOrder.Status = "sold";
                    }
                    else if (productToOrder.QuantityForSale > 0)
                    {
                        productToOrder.Status = "available";
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Товар '{productToOrder.Name}' успешно добавлен в заказ №{newPurchase.NumberPurchase}. Продавец получит уведомление.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Произошла ошибка при создании заказа: {ex.Message}.";
            }

            return RedirectToPage();
        }
    }
}