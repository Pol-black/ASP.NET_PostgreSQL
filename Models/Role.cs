using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public partial class Role
{
    [Display(Name = "ID роль")]
    public int IdRole { get; set; }

    [Required(ErrorMessage = "Название роли обязательно")]
    [StringLength(50, ErrorMessage = "Название роли не должно превышать 50 символов")]
    [RegularExpression(@"^[A-Za-zА-Яа-я\s]+$",
        ErrorMessage = "Название роли может содержать только буквы и пробелы")]
    [Display(Name = "Название роли")]
    public string RoleName { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
