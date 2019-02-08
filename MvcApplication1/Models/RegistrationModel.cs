using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication1.Models
{
    public class RegistrationModel
    {
        [Required(ErrorMessage = "Поле \"Имя\" обязательное!")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Имя должно быть от 3 до 40 символов")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Поле \"Почта\" обязательное!")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Почта должна быть от 3 до 255 символов")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле \"Пароль\" обязательное!")]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 32 символов")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Поле \"Повтор пароля\" обязательное!")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        public string confirm_password { get; set; }
    }
}