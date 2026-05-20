using PersonalFinance.Maui.Models;
using System.Net.Http.Json;

namespace PersonalFinance.Maui.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();

            _httpClient.BaseAddress =
                new Uri("http://localhost:5064/");
        }

        public async Task<User?> Login(string email, string password)
        {
            var response = await _httpClient.PostAsync(
                $"api/Users/login?email={email}&password={password}",
                null);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var user = await response.Content
                .ReadFromJsonAsync<User>();

            return user;
        }

        public async Task<bool> Register(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/register", user);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var response = await _httpClient.PutAsync(
                $"api/Users/change-password?userId={userId}&oldPassword={oldPassword}&newPassword={newPassword}",
                null);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Transaction>> GetTransactions(int userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Transaction>>($"api/Transactions/user/{userId}");
                return response ?? new List<Transaction>();
            }
            catch
            {
                return new List<Transaction>();
            }
        }

        public async Task<bool> AddTransaction(Transaction transaction)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Transactions", transaction);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTransaction(int id, Transaction transaction)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Transactions/{id}", transaction);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTransaction(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Transactions/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}