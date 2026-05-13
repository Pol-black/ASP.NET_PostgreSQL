using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public partial class Purchase
{
    [Display(Name = "ID заказа")]
    public int IdPurchase { get; set; }

    [Required(ErrorMessage = "Номер заказа обязателен")]
    [StringLength(20, ErrorMessage = "Номер заказа не должен превышать 20 символов")]
    [Display(Name = "Номер заказа")]
    public string NumberPurchase { get; set; } = null!;

    [Display(Name = "ID продавца")]
    public int IdSeller { get; set; }

    [Display(Name = "ID покупателя")]
    public int IdBuyer { get; set; }

    [Required(ErrorMessage = "Адрес отправления обязателен")]
    [StringLength(200, ErrorMessage = "Адрес отправления слишком длинный")]
    [Display(Name = "Адрес отправления")]
    public string AddressDeparture { get; set; } = null!;

    [Required(ErrorMessage = "Адрес получения обязателен")]
    [StringLength(200, ErrorMessage = "Адрес получения слишком длинный")]
    [Display(Name = "Адрес получения")]
    public string AddressReceiving { get; set; } = null!;

    [Required(ErrorMessage = "Способ доставки обязателен")]
    [StringLength(50, ErrorMessage = "Способ доставки не должен превышать 50 символов")]
    [RegularExpression(@"^(courier|pickup|mail)$",
        ErrorMessage = "Допустимые значения: courier, pickup, mail")]
    [Display(Name = "Способ доставки")]
    public string MethodDelivery { get; set; } = null!;

    [Required(ErrorMessage = "Дата создания обязательна")]
    [Display(Name = "Дата создания")]
    public DateOnly CreatedAt { get; set; }
    [Display(Name = "Покупатель")]
    public virtual Account IdBuyerNavigation { get; set; } = null!;
    [Display(Name = "Продавец")]
    public virtual Account IdSellerNavigation { get; set; } = null!;

    public virtual ICollection<ItemPurchase> ItemPurchases { get; set; } = new List<ItemPurchase>();
}
