using Microsoft.AspNetCore.Identity;

namespace FitnessApp.Web.Models
{
    // IdentityUser sınıfından miras alarak standart özellikleri (email, password) alıyoruz
    public class AppUser : IdentityUser
    {
        public string AdSoyad { get; set; }
        public DateTime KayitTarihi { get; set; } = DateTime.Now;
    }
}