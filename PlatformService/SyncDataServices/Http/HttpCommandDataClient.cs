using System.Text;
using System.Text.Json;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Http
{
    public class HttpCommandDataClient(HttpClient httpClient,IConfiguration configuration) : ICommandDataClient
    {
        private HttpClient _httpClient = httpClient;
        private IConfiguration _config = configuration;

        public async Task SendPlatformToCommand(PlatformDto platform)
        {
            var httpContent = new StringContent(
                JsonSerializer.Serialize(platform),
                Encoding.UTF8,
                "application/json");

            var url = $"{_config["CommandService"]}";

            Console.WriteLine(url);

            if (string.IsNullOrWhiteSpace(url))
                    throw new Exception("CommandService URL is not configured properly.");
    
            var response = await _httpClient.PostAsync(url, httpContent);

            if (response.IsSuccessStatusCode)
                Console.WriteLine("==> Sync POST to CommandService was OK!");
            else
                Console.WriteLine("==>---XX Sync Post failed---XX");    
        }
    }
}