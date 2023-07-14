using System.ComponentModel.DataAnnotations;

namespace Mirror.Models
{
    public class ProxyCredential
    {
        public int Id { get; set; }

        [Display(Name = "Ссылка на список прокси вида ip:port")]
        [Required(ErrorMessage = "Заполните обязательное поле")]
        public string ListUrl { get; set; }
    }
}
