using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessApp.Web.Data;
using FitnessApp.Web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace FitnessApp.Controllers
{
    // DİKKAT: Bu satır sayesinde giriş yapmayan kimse bu sayfaları göremez!
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager; // Kullanıcıyı tanımak için gerekli servis

        // Constructor (Yapıcı Metot): Servisleri içeri alıyoruz
        public AppointmentsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Appointments (Listeleme Sayfası)
        public async Task<IActionResult> Index()
        {
            // Veritabanı sorgusunu hazırlıyoruz (Henüz çalıştırmadık)
            var appointmentsQuery = _context.Appointments
                .Include(a => a.AppUser)  // Randevuyu alan üyeyi getir
                .Include(a => a.Service)  // Hizmet detayını getir
                .Include(a => a.Trainer); // Hoca detayını getir

            // EĞER GİREN KİŞİ ADMİN İSE:
            if (User.IsInRole("Admin"))
            {
                // Hiçbir filtreleme yapma, hepsini listele
                return View(await appointmentsQuery.ToListAsync());
            }
            // EĞER NORMAL ÜYE İSE:
            else
            {
                // Sadece kendi ID'si olanları filtrele
                var currentUserId = _userManager.GetUserId(User);
                var myAppointments = await appointmentsQuery
                                           .Where(a => a.AppUserId == currentUserId)
                                           .ToListAsync();
                return View(myAppointments);
            }
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.AppUser)
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Güvenlik Önlemi: Üye başkasının detayını URL'den girmeye çalışırsa engelle
            if (!User.IsInRole("Admin") && appointment.AppUserId != _userManager.GetUserId(User))
            {
                return Unauthorized(); // Yetkin yok hatası
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            // Admin değilse, kullanıcı seçimi yaptırmıyoruz. O yüzden AppUser listesine gerek yok.
            // Sadece hizmet ve hoca listelerini gönderiyoruz.
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Isim");
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "AdSoyad");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tarih,Saat,TrainerId,ServiceId")] Appointment appointment)
        {
            // --- 1. VALIDATION HATALARINI TEMİZLE ---
            // Bu alanları formdan beklemiyoruz veya biz dolduracağız, o yüzden hata vermesini engelliyoruz.
            ModelState.Remove("AppUser");
            ModelState.Remove("Trainer");
            ModelState.Remove("Service");
            ModelState.Remove("AppUserId");
            ModelState.Remove("Durum");
            // ----------------------------------------

            // 2. Tarih ve Saat atamaları
            appointment.OlusturulmaTarihi = DateTime.Now;
            appointment.AppUserId = _userManager.GetUserId(User); // ID'yi biz burada atıyoruz

            // 3. ÇAKIŞMA KONTROLÜ (Aynen kalsın)
            bool cakismaVarMi = _context.Appointments.Any(a =>
                a.TrainerId == appointment.TrainerId &&
                a.Tarih == appointment.Tarih &&
                a.Saat == appointment.Saat &&
                a.Id != appointment.Id);

            if (cakismaVarMi)
            {
                ModelState.AddModelError("", "Seçtiğiniz antrenörün bu saatte başka bir randevusu var.");
                TempData["Hata"] = "Bu saat dolu!";
            }

            // 4. ARTIK KAYIT YAPABİLİRİZ
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["Basarili"] = "Randevu talebiniz alındı! Onay için yöneticiye gönderildi.";
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa yine dropdownları doldur
            // Hataları görmek için debug kodu:
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Hata"] = "Eksik bilgi: " + string.Join(", ", errors);

            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "AdSoyad", appointment.TrainerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Isim", appointment.ServiceId);
            return View(appointment);
        }


        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Güvenlik: Başkasının randevusunu düzenlemeye kalkarsa engelle
            if (!User.IsInRole("Admin") && appointment.AppUserId != _userManager.GetUserId(User))
            {
                return Unauthorized();
            }

            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Isim", appointment.ServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "AdSoyad", appointment.TrainerId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tarih,Saat,AppUserId,TrainerId,ServiceId,OlusturulmaTarihi")] Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Isim", appointment.ServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "AdSoyad", appointment.TrainerId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.AppUser)
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Güvenlik: Başkasının randevusunu silmeye kalkarsa engelle
            if (!User.IsInRole("Admin") && appointment.AppUserId != _userManager.GetUserId(User))
            {
                return Unauthorized();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            // Silmeden önce son bir güvenlik kontrolü daha (backend tarafında)
            if (appointment != null)
            {
                if (!User.IsInRole("Admin") && appointment.AppUserId != _userManager.GetUserId(User))
                {
                    return Unauthorized();
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }

        // --- AJAX İÇİN API ---
        [HttpGet]
        public JsonResult GetMusaitSaatler(int trainerId, DateTime date)
        {
            // 1. Spor Salonu Mesai Saatleri (09:00 - 19:00 arası 2'şer saat)
            List<string> tumSaatler = new List<string>
            {
                "09:00", "11:00", "13:00", "15:00", "17:00", "19:00"
            };

            // 2. O gün, o hocanın dolu olduğu saatleri veritabanından çekelim
            var doluSaatler = _context.Appointments
                .Where(a => a.TrainerId == trainerId && a.Tarih.Date == date.Date)
                .Select(a => a.Saat)
                .ToList();

            // 3. Her saat için durum belirleyelim
            var saatDurumlari = tumSaatler.Select(saat => new
            {
                Saat = saat,
                DoluMu = doluSaatler.Contains(saat) // Eğer dolu listesindeyse true
            });

            return Json(saatDurumlari);
        }

    }

}