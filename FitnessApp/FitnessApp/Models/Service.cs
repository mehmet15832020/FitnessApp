using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Web.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [Display(Name = "Hizmet Adı")]
        public string Isim { get; set; } // Örn: Pilates, Yoga

        // --- YENİ EKLENEN ALANLAR ---
        [Required(ErrorMessage = "Süre bilgisi zorunludur.")]
        [Display(Name = "Süre (Dakika)")]
        public int Sure { get; set; } // Örn: 60 (dakika)

        [Required(ErrorMessage = "Ücret bilgisi zorunludur.")]
        [Display(Name = "Ücret (TL)")]
        public decimal Ucret { get; set; } // Örn: 500.00
        // ----------------------------

        // İlişkiler
        public List<Appointment>? Appointments { get; set; }
    }
}