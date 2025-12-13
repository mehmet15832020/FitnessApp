using FitnessApp.Web.Data;
using FitnessApp.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using FitnessApp.Web.Services;
var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Üyelik Sistemi (Identity) Ayarı
// (Otomatik eklenen hatalı 'DefaultIdentity' satırını sildik, doğrusu bu:)
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // İstersen şifre kurallarını buradan gevşetebilirsin (Test için kolaylık olur)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
    options.SignIn.RequireConfirmedAccount = false; // E-posta onayı zorunluluğunu kaldırdık
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
// Mail hatasını çözen sihirli satır:
builder.Services.AddTransient<IEmailSender, EmailSender>();
// Not: AddDefaultUI() eklememize gerek yok, Scaffolding zaten dosyaları üretti.

// 3. MVC ve Razor Sayfaları
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
// Yapay Zeka Servisi (Gemini)
builder.Services.AddScoped<FitnessApp.Web.Services.GeminiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// DİKKAT: Buradaki sıralama çok önemlidir!
app.UseAuthentication(); // <-- Önce: Kimlik Kontrolü (Sen kimsin?) (BUNU EKLEDİM)
app.UseAuthorization();  // <-- Sonra: Yetki Kontrolü (Girebilir misin?)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Razor sayfalarını (Login/Register) haritala

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Az önce yazdığımız DbSeeder'ı çalıştırıyoruz
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        // Hata olursa konsola yazsın (Opsiyonel)
        Console.WriteLine("Roller oluşturulurken hata çıktı: " + ex.Message);
    }
}

app.Run();