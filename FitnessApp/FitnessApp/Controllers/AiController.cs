using FitnessApp.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessApp.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        private readonly GeminiService _geminiService; // Değişen kısım

        public AiController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(int yas, double kilo, double boy, string cinsiyet, string hedef)
        {
            if (yas <= 0 || kilo <= 0 || boy <= 0)
            {
                ViewBag.Hata = "Lütfen tüm değerleri doğru giriniz.";
                return View();
            }

            // Servise git ve cevabı al
            ViewBag.Sonuc = await _geminiService.GetFitnessAdvice(yas, kilo, boy, cinsiyet, hedef);

            return View();
        }
    }
}