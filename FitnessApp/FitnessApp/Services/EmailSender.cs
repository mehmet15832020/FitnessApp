using Microsoft.AspNetCore.Identity.UI.Services;

namespace FitnessApp.Web.Services
{
    // Bu sınıf, sistem mail atmak istediğinde devreye girer.
    // Biz içini boş bıraktık ki hata vermesin ama mail de atmasın.
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Burası boş, yani mail atılmış gibi davranıp hiçbir şey yapmıyor.
            return Task.CompletedTask;
        }
    }
}