using System.Text;
using System.Text.Json;
using Cogtive.App.Models;

namespace Cogtive.App.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        
        public ApiService()
        {
            _httpClient = new HttpClient();
            // Special IP for Android emulator to access host machine
            _baseUrl = "http://localhost:5211/api";
            
            // For iOS simulator, uncomment this:
            // _baseUrl = "http://localhost:5000/api";
        }
        
        public async Task<List<Machine>> GetMachinesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/machines");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Machine>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        
        public async Task<Machine> GetMachineByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/machines/{id}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Machine>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        
        public async Task<List<ProductionData>> GetProductionDataAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/production-data");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProductionData>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        
        public async Task<List<ProductionData>> GetMachineProductionDataAsync(int machineId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/machines/{machineId}/production-data");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProductionData>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        
        public async Task<ProductionData> PostProductionDataAsync(ProductionData data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_baseUrl}/production-data", content);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductionData>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
