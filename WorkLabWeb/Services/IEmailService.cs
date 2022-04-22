using WorkLabWeb.ServiceModels;
using System.Threading.Tasks;

namespace WorkLabWeb.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmail(EmailRequest request);
    }
}