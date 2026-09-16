using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public partial class CatalogForMaterial
{
    public int IdMaterial { get; set; }

    [Required(ErrorMessage = "Название материала обязательно")]
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    [RegularExpression(@"^[A-Za-zА-Яа-я0-9\s\-]+$",
        ErrorMessage = "Название материала может содержать только буквы, пробел и дефис")]
    [Display(Name = "Название материала")]
    public string MaterialName { get; set; } = null!;

    [Range(0.01, 999999999.99, ErrorMessage = "Стоимость за единицу должна быть больше 0 и не превышать 999 999 999,99")]
    [Display(Name = "Цена материала")]
    public decimal CostPerUnit { get; set; }

    public virtual ICollection<ConnectProductMaterial> ConnectProductMaterials { get; set; } = new List<ConnectProductMaterial>();
}
