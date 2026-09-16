using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public partial class Account
{
    public int IdAccount { get; set; }

    [Required(ErrorMessage = "Имя аккаунта обязательно")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 50 символов")]
    [RegularExpression(@"^[A-Za-zА-Яа-я0-9\s\-_]+$", ErrorMessage = "Недопустимые символы в имени. Разрешены буквы, цифры, пробел, дефис и подчёркивание.")]
    [Display(Name = "Название аккаунта")]
    public string? AccountName { get; set; }
   
    [Required(ErrorMessage = "Адрес обязателен")]
    [StringLength(200, ErrorMessage = "Адрес не должен превышать 200 символов")]
    [Display(Name = "Адрес")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [StringLength(50, ErrorMessage = "Email не должен превышать 50 символов")]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Телефон обязателен")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "Телефон должен состоять ровно из 11 цифр")]
    [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Телефон должен состоять только из 11 цифр")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "Пароль обязателен")]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = null!;

    public int IdRole { get; set; }

    [Display(Name = "Роль")]
    public virtual Role? IdRoleNavigation { get; set; }

    public virtual ICollection<Product> ProductIdIndivBuyerNavigations { get; set; } = new List<Product>();
    public virtual ICollection<Product> ProductIdSellerNavigations { get; set; } = new List<Product>();
    public virtual ICollection<ProductionPurchase> ProductionPurchaseIdBuyerNavigations { get; set; } = new List<ProductionPurchase>();
    public virtual ICollection<ProductionPurchase> ProductionPurchaseIdSellerNavigations { get; set; } = new List<ProductionPurchase>();
    public virtual ICollection<Purchase> PurchaseIdBuyerNavigations { get; set; } = new List<Purchase>();
    public virtual ICollection<Purchase> PurchaseIdSellerNavigations { get; set; } = new List<Purchase>();
}
