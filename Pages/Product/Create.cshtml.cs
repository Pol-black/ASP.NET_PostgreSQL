using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System.ComponentModel.DataAnnotations;
using Npgsql; // ДОБАВЬТЕ ЭТУ СТРОКУ

namespace RazorPagesMovie.Pages.Product
{
    [Bind(
        "Product.Name",
        "Product.TypeArt",
        "Product.IdSeller",
        "Product.QuantityForSale",
        "Product.Price",
        "Product.Status",
        "Product.IdIndivBuyer"
    )]
    public class CreateModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;
        private readonly IConfiguration _configuration;

        public CreateModel(RazorPagesMovie.Models.ArtMarketDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult OnGet()
        {
            ReturnUrl = Request.Headers["Referer"].ToString() ?? "/Index";
            PopulateDropDowns();
            return Page();
        }

        [BindProperty]
        public Models.Product Product { get; set; } = default!;

        [BindProperty]
        [Display(Name = "Материалы для продукта")]
        public List<int> SelectedMaterialIds { get; set; } = new List<int>();

        [BindProperty]
        public List<MaterialQuantity> MaterialQuantities { get; set; } = new List<MaterialQuantity>();

        public SelectList MaterialsList { get; set; } = default!;
        public string ReturnUrl { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            ReturnUrl = Request.Headers["Referer"].ToString() ?? "/Index";

            if (!ModelState.IsValid)
            {
                PopulateDropDowns();
                return Page();
            }

            // Получаем строку подключения
            var connectionString = _configuration.GetConnectionString("DefaultConnection") ??
                                  _context.Database.GetConnectionString();

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Создаем продукт
                var productId = await CreateProductAsync(connection, transaction);

                // 2. Добавляем материалы
                if (SelectedMaterialIds != null && SelectedMaterialIds.Any())
                {
                    await AddMaterialsAsync(connection, transaction, productId);
                }

                await transaction.CommitAsync();

                // 3. Возвращаем успех
                TempData["SuccessMessage"] = "Продукт успешно создан!";
                return Redirect(ReturnUrl);
            }
            catch (PostgresException pgEx)
            {
                await transaction.RollbackAsync();

                // Обработка конкретных ошибок PostgreSQL
                var errorMessage = pgEx.SqlState switch
                {
                    "23505" => "Нарушение уникальности данных.",
                    "23503" => "Нарушение внешнего ключа. Проверьте существование связанных записей.",
                    "23514" => "Нарушение проверочного ограничения.",
                    "42P01" => "Отсутствует таблица или представление.",
                    _ => pgEx.Message
                };

                ModelState.AddModelError("", $"Ошибка базы данных: {errorMessage}");
                Console.WriteLine($"Postgres Error: {pgEx.SqlState} - {pgEx.Message}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Ошибка при создании продукта: {ex.Message}");
                Console.WriteLine($"Error: {ex}");
            }

            PopulateDropDowns();
            return Page();
        }

        private async Task<int> CreateProductAsync(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            var sql = @"
                INSERT INTO art_market_schema.product 
                (name, type_art, id_seller, quantity_for_sale, price, status, id_indiv_buyer) 
                VALUES (@name, @typeArt, @sellerId, @quantity, @price, @status, @buyerId) 
                RETURNING id_product";

            using var cmd = new NpgsqlCommand(sql, connection, transaction);

            cmd.Parameters.AddWithValue("@name", Product.Name);
            cmd.Parameters.AddWithValue("@typeArt", Product.TypeArt);
            cmd.Parameters.AddWithValue("@sellerId", Product.IdSeller);
            cmd.Parameters.AddWithValue("@quantity", Product.QuantityForSale);
            cmd.Parameters.AddWithValue("@price", Product.Price);
            cmd.Parameters.AddWithValue("@status", Product.Status);
            cmd.Parameters.AddWithValue("@buyerId",
                Product.IdIndivBuyer.HasValue && Product.IdIndivBuyer.Value > 0 ?
                (object)Product.IdIndivBuyer.Value : DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private async Task AddMaterialsAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, int productId)
        {
            // Временное отключение триггера
            await DisableTriggerAsync(connection, transaction);

            try
            {
                foreach (var materialId in SelectedMaterialIds)
                {
                    var quantityItem = MaterialQuantities
                        .FirstOrDefault(mq => mq?.MaterialId == materialId);

                    if (quantityItem != null && quantityItem.Quantity > 0)
                    {
                        var sql = @"
                            INSERT INTO art_market_schema.connect_product_material 
                            (id_product, id_material, quantity, unit) 
                            VALUES (@productId, @materialId, @quantity, @unit)";

                        using var cmd = new NpgsqlCommand(sql, connection, transaction);
                        cmd.Parameters.AddWithValue("@productId", productId);
                        cmd.Parameters.AddWithValue("@materialId", materialId);
                        cmd.Parameters.AddWithValue("@quantity", quantityItem.Quantity);
                        cmd.Parameters.AddWithValue("@unit", quantityItem.Unit ?? "шт");

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            finally
            {
                // Восстанавливаем триггер
                await EnableTriggerAsync(connection, transaction);
            }
        }

        private async Task DisableTriggerAsync(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            try
            {
                var disableSql = @"
                    DROP TRIGGER IF EXISTS trg_log_cost_on_material_change 
                    ON art_market_schema.connect_product_material";

                using var cmd = new NpgsqlCommand(disableSql, connection, transaction);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось отключить триггер: {ex.Message}");
            }
        }

        private async Task EnableTriggerAsync(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            try
            {
                var enableSql = @"
                    CREATE TRIGGER trg_log_cost_on_material_change
                    AFTER INSERT OR UPDATE OR DELETE ON art_market_schema.connect_product_material
                    FOR EACH ROW
                    EXECUTE FUNCTION art_market_schema.log_product_cost()";

                using var cmd = new NpgsqlCommand(enableSql, connection, transaction);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось восстановить триггер: {ex.Message}");
            }
        }

        private void PopulateDropDowns()
        {
            // 1. Покупатели (IdIndivBuyer)
            var buyers = _context.Accounts
                                 .Include(a => a.IdRoleNavigation)
                                 .Where(a => a.IdRoleNavigation != null && a.IdRoleNavigation.RoleName == "buyer")
                                 .ToList();

            var buyersSelectList = new SelectList(buyers, "IdAccount", "AccountName");
            var items = buyersSelectList.ToList();
            items.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "— Выберите покупателя индивидуального заказа (Необязательно) —",
                Selected = true
            });
            ViewData["IdIndivBuyer"] = items;

            // 2. Продавцы (IdSeller)
            var sellers = _context.Accounts
                                  .Include(a => a.IdRoleNavigation)
                                  .Where(a => a.IdRoleNavigation != null && a.IdRoleNavigation.RoleName == "seller")
                                  .ToList();
            ViewData["IdSeller"] = new SelectList(sellers, "IdAccount", "AccountName");

            // 3. Материалы
            var materials = _context.CatalogForMaterials
                                   .Select(m => new { m.IdMaterial, m.MaterialName })
                                   .ToList();
            MaterialsList = new SelectList(materials, "IdMaterial", "MaterialName");
        }
    }

    // Вспомогательный класс для хранения количества и единиц измерения
    public class MaterialQuantity
    {
        public int MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
    }
}