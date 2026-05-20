using System;
using PersonalFinance.Maui.Services;

namespace PersonalFinance.Maui.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _apiService;

        public LoginPage()
        {
            InitializeComponent();
            _apiService = new ApiService();

            LoadingLayout.IsVisible = false;
            LoginActivityIndicator.IsRunning = false;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert(
                    "Hata",
                    "E-posta ve şifre alanları boş bırakılamaz.",
                    "Tamam");
                return;
            }

            try
            {
                LoadingLayout.IsVisible = true;
                LoginActivityIndicator.IsRunning = true;

                var user = await _apiService.Login(email, password);

                if (user == null)
                {
                    await DisplayAlert(
                        "Hata",
                        "E-posta veya şifre hatalı.",
                        "Tamam");
                    return;
                }

                if (Application.Current != null)
                {
                    Application.Current.MainPage = new NavigationPage(new DashboardPage(user));
                }
            }
            catch
            {
                await DisplayAlert(
                    "Hata",
                    "Giriş sırasında bir bağlantı hatası oluştu. API'nin çalıştığından emin olun.",
                    "Tamam");
            }
            finally
            {
                LoadingLayout.IsVisible = false;
                LoginActivityIndicator.IsRunning = false;
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}