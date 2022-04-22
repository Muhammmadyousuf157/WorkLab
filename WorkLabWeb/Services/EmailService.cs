using WorkLabWeb.ServiceModels;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WorkLabWeb.Services
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient _httpClient;

        public EmailService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> SendEmail(EmailRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/sendEmail", request).ConfigureAwait(false);

            return response.IsSuccessStatusCode;
        }
    }
}