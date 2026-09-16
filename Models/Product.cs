using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public partial class Product
{
    public int IdProduct { get; set; }

    [Required(ErrorMessage = "Название товара обязательно")]
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    [RegularExpression(@"^[A-Za-zА-Яа-я0-9\s\-.,!?]+$",
        ErrorMessage = "Недопустимые символы в названии. Разрешены буквы, цифры, пробел и знаки препинания")]
    [Display(Name = "Название товара")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Тип искусства обязателен")]
    [StringLength(100, ErrorMessage = "Тип искусства не должен превышать 100 символов")]
    [RegularExpression(@"^[A-Za-zА-Яа-я\s]+$",
        ErrorMessage = "Тип искусства может содержать только буквы, пробел и дефис")]
    [Display(Name = "Тип искусства")]
    public string TypeArt { get; set; } = null!;

    [Display(Name = "ID продавца")]
    public int IdSeller { get; set; }

    [Range(0, 10000, ErrorMessage = "Количество для продажи должно быть от 0 до 10 000")]
    [Display(Name = "Количество товара")]
    public int QuantityForSale { get; set; }

    [Range(0.01, 999999999999.99, ErrorMessage = "Цена должна быть больше 0 и не превышать 999 999 999 999,99")]
    [Display(Name = "Цена")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Статус товара обязателен")]
    [StringLength(30, ErrorMessage = "Статус не должен превышать 30 символов")]
    [RegularExpression(@"^(available|reserved|sold)$",
        ErrorMessage = "Допустимые значения статуса: available, reserved, sold (в нижнем регистре)")]
    [Display(Name = "Статус")]
    public string Status { get; set; } = null!;



    [Display(Name = "Индивид.товар покупателя")]
    public int? IdIndivBuyer { get; set; }

    public virtual ICollection<ConnectProductMaterial> ConnectProductMaterials { get; set; } = new List<ConnectProductMaterial>();

    public virtual Account? IdIndivBuyerNavigation { get; set; }

    public virtual Account? IdSellerNavigation { get; set; }

    public virtual ICollection<ItemPurchase> ItemPurchases { get; set; } = new List<ItemPurchase>();

    public virtual ICollection<ProductionPurchase> ProductionPurchases { get; set; } = new List<ProductionPurchase>();
}
