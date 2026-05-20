using System;
using PersonalFinance.Maui.Models;
using PersonalFinance.Maui.Services;

namespace PersonalFinance.Maui.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly ApiService _apiService;

        public RegisterPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var fullName = FullNameEntry.Text;
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;
            var confirmPassword = ConfirmPasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                await DisplayAlert("Hata", "Lütfen tüm alanları doldurun.", "Tamam");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Hata", "Şifreler uyuşmuyor.", "Tamam");
                return;
            }

            var newUser = new User
            {
                FullName = fullName,
                Email = email,
                Password = password
            };

            var success = await _apiService.Register(newUser);

            if (success)
            {
                await DisplayAlert("Başarılı", "Kayıt işlemi başarılı! Giriş yapabilirsiniz.", "Tamam");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Hata", "Kayıt işlemi başarısız oldu. E-posta adresi zaten kullanımda olabilir.", "Tamam");
            }
        }

        private async void OnBackToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
