using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    public class OpenAIService
    {
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly HttpClient _httpClient;

        public OpenAIService(string apiKey, string endpoint = "https://api.openai.com/v1/chat/completions")
        {
            _apiKey = apiKey;
            _endpoint = endpoint;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> GetBudgetRecommendationAsync(string history)
        {
            var prompt = $"Na z�klad� t�to historie v�daj� navrhni rozpo�et: {history}";
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[] {
                    new { role = "system", content = "Jsi finan�n� asistent." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 300
            };
            var json = JsonSerializer.Serialize(requestBody);
            var response = await _httpClient.PostAsync(_endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var result = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            return result;
        }

        public async Task<string> AnalyzeTransactionsAsync(string history)
        {
            var prompt = $"Analyzuj tyto transakce a detekuj neobvykl� nebo podez�el� v�daje: {history}";
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[] {
                    new { role = "system", content = "Jsi finan�n� analytik." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 300
            };
            var json = JsonSerializer.Serialize(requestBody);
            var response = await _httpClient.PostAsync(_endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var result = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            return result;
        }

        public async Task<string> ChatbotAsync(string userMessage)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[] {
                    new { role = "system", content = "Jsi finan�n� asistent. Pom�hej s ovl�d�n�m aplikace, n�pov�dou a tipy." },
                    new { role = "user", content = userMessage }
                },
                max_tokens = 300
            };
            var json = JsonSerializer.Serialize(requestBody);
            var response = await _httpClient.PostAsync(_endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var result = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            return result;
        }

        public async Task<string> CategorizeTransactionAsync(string description)
        {
            var prompt = $"Do jak� kategorie pat�� tato transakce? Popis: {description}. Odpov�z pouze kategori�.";
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[] {
                    new { role = "system", content = "Jsi finan�n� asistent." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 50
            };
            var json = JsonSerializer.Serialize(requestBody);
            var response = await _httpClient.PostAsync(_endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var result = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            return result;
        }
    }
}
