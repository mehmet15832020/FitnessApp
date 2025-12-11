using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Web.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Display(Name = "Ad Soyad")]
        [Required(ErrorMessage = "Antrenör adı zorunludur")]
        public string AdSoyad { get; set; }

        [Display(Name = "Uzmanlık Alanı")]
        public string UzmanlikAlani { get; set; } // Örn: Vücut Geliştirme

        [Display(Name = "Fotoğraf")]
        public string? ResimUrl { get; set; } // Antrenörün resmi için
    }
}