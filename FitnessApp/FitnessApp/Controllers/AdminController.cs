using FitnessApp.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Controllers
{
    [Authorize(Roles = "Admin")] // SADECE ADMİN GİREBİLİR!
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Basit İstatistikler
            ViewBag.ToplamUye = _context.Users.Count();
            ViewBag.ToplamAntrenor = _context.Trainers.Count();
            ViewBag.ToplamRandevu = _context.Appointments.Count();

            // Onay Bekleyen Randevular
            var bekleyenRandevular = _context.Appointments
                .Include(a => a.AppUser)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.Durum == "Onay Bekliyor")
                .ToList();

            return View(bekleyenRandevular);
        }

        // Randevu Onaylama İşlemi
        public IActionResult Onayla(int id)
        {
            var randevu = _context.Appointments.Find(id);
            if (randevu != null)
            {
                randevu.Durum = "Onaylandı";
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Randevu İptal İşlemi
        public IActionResult Iptal(int id)
        {
            var randevu = _context.Appointments.Find(id);
            if (randevu != null)
            {
                _context.Appointments.Remove(randevu);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}