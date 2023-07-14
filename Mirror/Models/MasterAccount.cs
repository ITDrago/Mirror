using System.ComponentModel.DataAnnotations;

namespace Mirror.Models
{
    public class MasterAccount
    {
        public int Id { get; set; }

        [Display(Name = "Логин")]
        [Required(ErrorMessage = "Заполните обязательное поле")]
        public string Login { get; set; }

        [Display(Name = "Пароль")]
        [Required(ErrorMessage = "Заполните обязательное поле")]
        public string Password { get; set; }
    }
}
