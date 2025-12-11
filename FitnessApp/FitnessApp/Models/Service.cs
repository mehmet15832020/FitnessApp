using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Web.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Display(Name = "Hizmet Adı")]
        [Required(ErrorMessage = "Hizmet adı boş geçilemez")]
        public string Isim { get; set; } // Örn: Pilates

        [Display(Name = "Süre (Dakika)")]
        public int Sure { get; set; } // Örn: 60

        [Display(Name = "Ücret (TL)")]
        public decimal Ucret { get; set; }
    }
}