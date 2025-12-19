using FitnessApp.Web.Data;
using FitnessApp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FitnessApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FitnessApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. TÜM ANTRENÖRLERİ GETİRME (GET: api/FitnessApi/Trainers)
        [HttpGet("Trainers")]
        public async Task<ActionResult<IEnumerable<Trainer>>> GetTrainers()
        {
            // LINQ Kullanımı: Veritabanından listeye çevirir
            return await _context.Trainers.ToListAsync();
        }

        // 2. FİLTRELEME İŞLEMİ (GET: api/FitnessApi/Trainers/Yoga)
        // Uzmanlık alanına göre hocaları getirir (LINQ Where şartı)
        [HttpGet("Trainers/{uzmanlik}")]
        public async Task<ActionResult<IEnumerable<Trainer>>> GetTrainersBySpecialty(string uzmanlik)
        {
            // LINQ SORGUSU: Uzmanlık alanı girilen kelimeyi içerenleri getir
            var trainers = await _context.Trainers
                .Where(t => t.UzmanlikAlani.Contains(uzmanlik))
                .ToListAsync();

            if (trainers == null || trainers.Count == 0)
            {
                return NotFound("Bu uzmanlık alanında antrenör bulunamadı.");
            }

            return trainers;
        }

        // 3. TÜM HİZMETLERİ GETİRME (GET: api/FitnessApi/Services)
        [HttpGet("Services")]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            return await _context.Services.ToListAsync();
        }

        // --- YENİ EKLENEN: DASHBOARD İSTATİSTİKLERİ ---
        [HttpGet("Stats")]
        public IActionResult GetDashboardStats()
        {
            // 1. Toplam Eğitmen Sayısı (Basit Count)
            var totalTrainers = _context.Trainers.Count();

            // 2. Toplam Randevu Sayısı
            var totalAppointments = _context.Appointments.Count();

            // 3. Toplam Beklenen Kazanç (SUM - Toplama İşlemi)
            // Randevuların hizmetlerine gidip ücretlerini topluyoruz.
            var totalRevenue = _context.Appointments
                .Include(a => a.Service)
                .Sum(a => a.Service.Ucret);

            // 4. En Popüler Hizmet (GROUP BY - Gruplama ve Sıralama)
            // Randevuları hizmet ismine göre grupla -> Sayılarını bul -> Çoktan aza sırala -> İlkini al
            var mostPopularService = _context.Appointments
                .Include(a => a.Service)
                .GroupBy(a => a.Service.Isim)
                .Select(g => new { HizmetAdi = g.Key, Sayi = g.Count() })
                .OrderByDescending(x => x.Sayi)
                .FirstOrDefault();

            return Ok(new
            {
                Trainers = totalTrainers,
                Appointments = totalAppointments,
                Revenue = totalRevenue,
                PopularService = mostPopularService?.HizmetAdi ?? "Henüz Yok" // Veri yoksa hata vermesin
            });
        }
    }
}