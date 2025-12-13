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
    }
}