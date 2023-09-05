using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OfferAndFindAPI.Models
{
    public partial class User
    {
        public int? IdUser { get; set; }
        public string? Firstname { get; set; }
        public int? IdRole { get; set; }
        public int? IdStatus { get; set; }
        public string? Name { get; set; }
        public string? Patronymic { get; set; }

        [Required(ErrorMessage = "Не указан логин!")]
        public string? Login { get; set; } = null!;

        [Required(ErrorMessage = "Не указан пароль!")]
        public string? Password { get; set; } = null!;

        [Required(ErrorMessage = "Не указана почта!")]
        public string? EMail { get; set; } = null!;
    }
}
