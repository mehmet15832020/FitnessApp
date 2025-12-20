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

        [HttpGet("Stats")]
        public IActionResult GetDashboardStats()
        {
            // Şu anki zaman bilgileri
            var today = DateTime.Now;
            var currentMonth = today.Month;
            var currentYear = today.Year;

            // 1. TOPLAM CİRO (Sadece BU AY ve ONAYLANMIŞ olanlar)
            // Mantık: Tarihi bu ayın içinde olan VE Durumu "Onaylandı" olanların ücretini topla.
            var monthlyRevenue = _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.Tarih.Month == currentMonth &&
                            a.Tarih.Year == currentYear &&
                            a.Durum == "Onaylandı")
                .Sum(a => a.Service.Ucret);

            // 2. BEKLEYEN RANDEVULAR (Adminin onaylaması gerekenler)
            // Bu veri admin için paradan daha önemlidir, iş yükünü gösterir.
            var pendingAppointments = _context.Appointments
                .Count(a => a.Durum == "Onay Bekliyor");

            // 3. TOPLAM EĞİTMEN SAYISI
            var totalTrainers = _context.Trainers.Count();

            // 4. EN POPÜLER HİZMET (Tüm zamanların)
            var mostPopularService = _context.Appointments
                .Include(a => a.Service)
                .GroupBy(a => a.Service.Isim)
                .Select(g => new { HizmetAdi = g.Key, Sayi = g.Count() })
                .OrderByDescending(x => x.Sayi)
                .FirstOrDefault();

            return Ok(new
            {
                Trainers = totalTrainers,
                PendingCount = pendingAppointments, // Değişti: Toplam randevu yerine Bekleyenler
                Revenue = monthlyRevenue,           // Değişti: Sadece bu ayın cirosu
                PopularService = mostPopularService?.HizmetAdi ?? "Yok"
            });
        }
    }
}