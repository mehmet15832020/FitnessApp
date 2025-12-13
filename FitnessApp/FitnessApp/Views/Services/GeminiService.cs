
using System.Text;
using Newtonsoft.Json;

namespace FitnessApp.Web.Services
{
    public class GeminiService
    {
        // ⚠️ BURAYA KENDİ API ANAHTARINI YAPIŞTIR
        private readonly string _apiKey = "AIzaSyB_RRjUWTNrmd6GU3M8KZQ1L_VmME4NPas";

        // GÜNCELLENDİ: Listede gördüğümüz çalışan model (Gemini 2.0 Flash)
        // Listende 'gemini-flash-latest' olarak geçen ücretsiz model
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent";

        public async Task<string> GetFitnessAdvice(int yas, double kilo, double boy, string cinsiyet, string hedef)
        {
            var client = new HttpClient();

            // Yapay Zekaya talimatımız (Prompt)
            var prompt = $"Ben {yas} yaşında, {kilo} kg ağırlığında, {boy} cm boyunda bir {cinsiyet} bireyim. " +
                         $"Amacım: {hedef}. " +
                         $"Bana 1 günlük örnek diyet listesi (Kahvaltı, Öğle, Akşam) ve yapmam gereken 3 temel egzersizi maddeler halinde yaz. " +
                         $"Cevabı HTML formatında (<b>, <ul>, <li> etiketleri kullanarak) ver. Başlıkları h4 ile yaz.";

            // Gemini API'nin beklediği JSON yapısı
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            // İsteği gönder
            var response = await client.PostAsync($"{_apiUrl}?key={_apiKey}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseString);

                try
                {
                    // Cevabı JSON'dan çekip alıyoruz
                    return result.candidates[0].content.parts[0].text;
                }
                catch
                {
                    return "Yapay zeka cevap verdi ama formatı okuyamadım.";
                }
            }
            else
            {
                // Hata durumunda detay göster
                var errorMsg = await response.Content.ReadAsStringAsync();
                return $"BAĞLANTI HATASI: {response.StatusCode} - {errorMsg}";
            }
        }
    }
}