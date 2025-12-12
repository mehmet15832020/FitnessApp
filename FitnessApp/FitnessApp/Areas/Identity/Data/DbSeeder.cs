using FitnessApp.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace FitnessApp.Web.Data // Namespace'ine dikkat et, Data klasöründeyse FitnessApp.Web.Data olsun
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            var userManager = service.GetService<UserManager<AppUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // 1. Rolleri oluştur
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Member"))
                await roleManager.CreateAsync(new IdentityRole("Member"));

            // 2. Admin Kullanıcısı
            var adminEmail = "b211210001@sakarya.edu.tr"; // Numaranı kontrol et
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            // Kullanıcı yoksa oluştur
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    AdSoyad = "Sistem Yöneticisi",
                    EmailConfirmed = true,
                    KayitTarihi = DateTime.Now
                };
                await userManager.CreateAsync(adminUser, "sau"); // Şifre: sau
            }

            // *** KRİTİK GÜNCELLEME: Kullanıcı varsa bile rolü kontrol et ve ver ***
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}