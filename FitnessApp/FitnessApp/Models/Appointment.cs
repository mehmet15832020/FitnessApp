using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Web.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Display(Name = "Randevu Tarihi")]
        public DateTime Tarih { get; set; }

        [Display(Name = "Saat")]
        public string Saat { get; set; } // Örn: "14:00"

        // --- İlişkiler ---

        // Randevuyu alan üye
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        // Seçilen Antrenör
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        // Seçilen Hizmet
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
    }
}