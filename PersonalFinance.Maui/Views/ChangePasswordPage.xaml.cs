using PersonalFinance.Maui.Models;
using PersonalFinance.Maui.Services;
using System;
using Microsoft.Maui.Controls;

namespace PersonalFinance.Maui.Views
{
    public partial class ChangePasswordPage : ContentPage
    {
        private readonly User _user;
        private readonly ApiService _apiService;

        public ChangePasswordPage(User user)
        {
            InitializeComponent();
            _user = user;
            _apiService = new ApiService();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            string oldPassword = OldPasswordEntry.Text?.Trim() ?? string.Empty;
            string newPassword = NewPasswordEntry.Text?.Trim() ?? string.Empty;
            string confirmPassword = ConfirmNewPasswordEntry.Text?.Trim() ?? string.Empty;

            // 1. Boşluk Kontrolü
            if (string.IsNullOrWhiteSpace(oldPassword) || 
                string.IsNullOrWhiteSpace(newPassword) || 
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                await DisplayAlert("Hata", "Lütfen tüm alanları doldurun.", "Tamam");
                return;
            }

            // 2. Karakter Sayısı Kontrolü
            if (newPassword.Length < 6)
            {
                await DisplayAlert("Hata", "Yeni şifre en az 6 karakter olmalıdır.", "Tamam");
                return;
            }

            // 3. Şifrelerin Eşleşme Kontrolü
            if (newPassword != confirmPassword)
            {
                await DisplayAlert("Hata", "Yeni şifre ile yeni şifre tekrarı eşleşmiyor.", "Tamam");
                return;
            }

            try
            {
                // 4. API İstek
                bool isSuccess = await _apiService.ChangePassword(_user.Id, oldPassword, newPassword);

                if (isSuccess)
                {
                    await DisplayAlert("Başarılı", "Şifreniz başarıyla güncellendi.", "Tamam");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Hata", "Eski şifreniz hatalı veya şifre güncellenemedi.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Bir hata oluştu: {ex.Message}", "Tamam");
            }
        }
    }
}
