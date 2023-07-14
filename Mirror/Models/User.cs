using System;
using System.ComponentModel.DataAnnotations;

namespace Mirror.Models
{
    public class User
    {
        public int Id { get; set; }

        [Display(Name = "Имя пользователя")]
        [Required(ErrorMessage = "Заполните обязательное поле")]
        public string Username { get; set; }

        [Display(Name = "Логин")]
        [Required(ErrorMessage = "Заполните обязательное поле")]
        public string Login { get; set; }

        [Display(Name = "Пароль")]
        [Required(ErrorMessage = "Заполните обязательное поле")]
        public string Password { get; set; }

        [Display(Name = "Дата окончания действия")]
        [Required(ErrorMessage = "Заполните обязательное поле")]
        public DateTime DateOfEnd { get; set; }

        [Display(Name = "Вечный аккаунт")]
        public bool Eternal { get; set; }

        [Display(Name = "Администратор")]
        public bool IsAdmin { get; set; }
    }
}
