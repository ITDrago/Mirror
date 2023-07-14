using System.ComponentModel.DataAnnotations;

namespace Mirror.Models
{
    public class CssRule
    {
        public int Id { get; set; }

        [Display(Name = "Название правила")]
        [Required(ErrorMessage = "Заполните обязательное поле")]
        public string Name { get; set; }

        [Display(Name = "Регулярное выражение адреса")]
        [Required(ErrorMessage = "Заполните обязательное поле")]
        public string Address { get; set; }

        [Display(Name = "CSS код")]
        [Required(ErrorMessage = "Заполните обязательное поле")]
        public string CssCode { get; set; }
    }
}
