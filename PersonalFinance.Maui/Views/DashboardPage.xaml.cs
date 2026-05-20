using PersonalFinance.Maui.Models;
using PersonalFinance.Maui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace PersonalFinance.Maui.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly User _user;
        private readonly ApiService _apiService;
        private bool _isNavigating = false;

        public DashboardPage(User user)
        {
            InitializeComponent();
            _user = user;
            _apiService = new ApiService();

            if (App.Current != null && App.Current.UserAppTheme == AppTheme.Unspecified)
            {
                App.Current.UserAppTheme = AppTheme.Light;
            }

            ApplyTheme(App.Current?.UserAppTheme == AppTheme.Dark);
            UpdateThemeButtonText();

            WelcomeLabel.Text = $"Merhaba, {_user.FullName}! 👋";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
            await AnimateEntranceAsync();
        }

        private async Task AnimateEntranceAsync()
        {
            WelcomeSection.Opacity = 0;
            WelcomeSection.TranslationY = 15;
            BalanceCard.Opacity = 0;
            BalanceCard.TranslationY = 15;
            StatsGrid.Opacity = 0;
            StatsGrid.TranslationY = 15;
            RecentTransactionsHeader.Opacity = 0;
            RecentTransactionsHeader.TranslationY = 15;
            TransactionsListLayout.Opacity = 0;
            TransactionsListLayout.TranslationY = 15;
            EmptyViewLayout.Opacity = 0;
            EmptyViewLayout.TranslationY = 15;
            ActionButtonsGrid.Opacity = 0;
            ActionButtonsGrid.TranslationY = 15;

            _ = WelcomeSection.FadeTo(1, 200, Easing.CubicOut);
            _ = WelcomeSection.TranslateTo(0, 0, 200, Easing.CubicOut);

            await Task.Delay(40);
            _ = BalanceCard.FadeTo(1, 250, Easing.CubicOut);
            _ = BalanceCard.TranslateTo(0, 0, 250, Easing.CubicOut);

            await Task.Delay(40);
            _ = StatsGrid.FadeTo(1, 250, Easing.CubicOut);
            _ = StatsGrid.TranslateTo(0, 0, 250, Easing.CubicOut);

            await Task.Delay(40);
            _ = RecentTransactionsHeader.FadeTo(1, 250, Easing.CubicOut);
            _ = RecentTransactionsHeader.TranslateTo(0, 0, 250, Easing.CubicOut);

            if (TransactionsListLayout.IsVisible)
            {
                _ = TransactionsListLayout.FadeTo(1, 250, Easing.CubicOut);
                _ = TransactionsListLayout.TranslateTo(0, 0, 250, Easing.CubicOut);
            }
            else
            {
                _ = EmptyViewLayout.FadeTo(1, 250, Easing.CubicOut);
                _ = EmptyViewLayout.TranslateTo(0, 0, 250, Easing.CubicOut);
            }

            await Task.Delay(40);
            _ = ActionButtonsGrid.FadeTo(1, 250, Easing.CubicOut);
            _ = ActionButtonsGrid.TranslateTo(0, 0, 250, Easing.CubicOut);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var transactions = await _apiService.GetTransactions(_user.Id);

                decimal totalIncome = 0;
                decimal totalExpense = 0;

                foreach (var t in transactions)
                {
                    if (string.Equals(t.Type, "Gelir", StringComparison.OrdinalIgnoreCase))
                        totalIncome += t.Amount;
                    else if (string.Equals(t.Type, "Gider", StringComparison.OrdinalIgnoreCase))
                        totalExpense += t.Amount;
                }

                decimal totalBalance = totalIncome - totalExpense;

                TotalBalanceLabel.Text = $"{totalBalance:N2} ₺";
                TotalIncomeLabel.Text = $"{totalIncome:N2} ₺";
                TotalExpenseLabel.Text = $"{totalExpense:N2} ₺";

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var currentMonthTransactions = transactions
                    .Where(t =>
                        t.TransactionDate.Month == currentMonth &&
                        t.TransactionDate.Year == currentYear)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(5)
                    .ToList();

                if (currentMonthTransactions.Count == 0)
                {
                    EmptyViewLayout.IsVisible = true;
                    TransactionsListLayout.IsVisible = false;
                    BindableLayout.SetItemsSource(TransactionsListLayout, null);
                    return;
                }

                EmptyViewLayout.IsVisible = false;
                TransactionsListLayout.IsVisible = true;

                var displayList = currentMonthTransactions
                    .Select(t => new TransactionDisplayItem
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Category = t.Category,
                        DateFormatted = t.TransactionDate.ToString("dd.MM.yyyy HH:mm"),
                        AmountFormatted = string.Equals(t.Type, "Gelir", StringComparison.OrdinalIgnoreCase)
                            ? $"+{t.Amount:N2} ₺"
                            : $"-{t.Amount:N2} ₺",
                        AmountColor = string.Equals(t.Type, "Gelir", StringComparison.OrdinalIgnoreCase)
                            ? Color.FromArgb("#10B981")
                            : Color.FromArgb("#EF4444"),
                        CategoryIcon = GetCategoryIcon(t.Category)
                    })
                    .ToList();

                BindableLayout.SetItemsSource(TransactionsListLayout, displayList);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Veriler yüklenirken bir hata oluştu: " + ex.Message, "Tamam");
            }
        }

        private string GetCategoryIcon(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return "💳";

            return category.ToLower() switch
            {
                "maaş" or "maas" or "gelir" => "💰",
                "mutfak" or "yemek" or "gıda" or "gida" => "🍔",
                "fatura" or "faturalar" or "aidat" => "📄",
                "kira" or "ev" => "🏠",
                "ulaşım" or "ulasim" or "yol" or "araba" => "🚗",
                "eğlence" or "eglence" or "sinema" or "hobi" => "🎮",
                "sağlık" or "saglik" or "eczane" or "hastane" => "🏥",
                "giyim" or "alisveris" or "alışveriş" => "👕",
                _ => "💳"
            };
        }

        private async void OnAddTransactionClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Navigation.PushAsync(new AddTransactionPage(_user));
            }
            finally
            {
                _isNavigating = false;
            }
        }

        private async void OnAllTransactionsClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Navigation.PushAsync(new TransactionListPage(_user));
            }
            finally
            {
                _isNavigating = false;
            }
        }

        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Navigation.PushAsync(new ChangePasswordPage(_user));
            }
            finally
            {
                _isNavigating = false;
            }
        }

        private async void OnStatisticsClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                await Navigation.PushAsync(new StatisticsPage(_user));
            }
            finally
            {
                _isNavigating = false;
            }
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                if (Application.Current != null)
                {
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                }
            }
            finally
            {
                _isNavigating = false;
            }
        }

        private void OnThemeToggleClicked(object sender, EventArgs e)
        {
            if (App.Current == null) return;

            bool isDark = App.Current.UserAppTheme == AppTheme.Dark;
            bool targetDark = !isDark;

            App.Current.UserAppTheme = targetDark ? AppTheme.Dark : AppTheme.Light;
            ApplyTheme(targetDark);
            UpdateThemeButtonText();
        }

        private void UpdateThemeButtonText()
        {
            if (App.Current != null)
            {
                ThemeToggleButton.Text = App.Current.UserAppTheme == AppTheme.Dark
                    ? "☀️ Tema"
                    : "🌙 Tema";
            }
        }

        private void ApplyTheme(bool isDark)
        {
            if (Application.Current == null) return;

            var resources = Application.Current.Resources;

            if (isDark)
            {
                resources["PageBackground"] = Color.FromArgb("#0F0A1C");
                resources["CardBackground"] = Color.FromArgb("#18122B");
                resources["InputBackground"] = Color.FromArgb("#251B45");
                resources["TextPrimary"] = Color.FromArgb("#F5F3FF");
                resources["TextSecondary"] = Color.FromArgb("#C084FC");
                resources["TextMuted"] = Color.FromArgb("#7C7598");
                resources["Primary"] = Color.FromArgb("#8B5CF6");
                resources["PrimaryDark"] = Color.FromArgb("#F5F3FF");
                resources["Secondary"] = Color.FromArgb("#2E1065");
                resources["BorderSoft"] = Color.FromArgb("#3B2273");

                resources["PrimaryBrush"] = new SolidColorBrush(Color.FromArgb("#8B5CF6"));
                resources["PrimaryDarkBrush"] = new SolidColorBrush(Color.FromArgb("#F5F3FF"));
                resources["SecondaryBrush"] = new SolidColorBrush(Color.FromArgb("#2E1065"));
                resources["PageBackgroundBrush"] = new SolidColorBrush(Color.FromArgb("#0F0A1C"));
                resources["CardBackgroundBrush"] = new SolidColorBrush(Color.FromArgb("#18122B"));
                resources["InputBackgroundBrush"] = new SolidColorBrush(Color.FromArgb("#251B45"));
                resources["TextPrimaryBrush"] = new SolidColorBrush(Color.FromArgb("#F5F3FF"));
                resources["TextSecondaryBrush"] = new SolidColorBrush(Color.FromArgb("#C084FC"));
                resources["TextMutedBrush"] = new SolidColorBrush(Color.FromArgb("#7C7598"));
                resources["BorderSoftBrush"] = new SolidColorBrush(Color.FromArgb("#3B2273"));
            }
            else
            {
                resources["PageBackground"] = Color.FromArgb("#F4F0FF");
                resources["CardBackground"] = Color.FromArgb("#FFFFFF");
                resources["InputBackground"] = Color.FromArgb("#F5F3FF");
                resources["TextPrimary"] = Color.FromArgb("#1E1B4B");
                resources["TextSecondary"] = Color.FromArgb("#7C7598");
                resources["TextMuted"] = Color.FromArgb("#A39FBA");
                resources["Primary"] = Color.FromArgb("#6D28D9");
                resources["PrimaryDark"] = Color.FromArgb("#4C1D95");
                resources["Secondary"] = Color.FromArgb("#EDE9FE");
                resources["BorderSoft"] = Color.FromArgb("#E9D5FF");

                resources["PrimaryBrush"] = new SolidColorBrush(Color.FromArgb("#6D28D9"));
                resources["PrimaryDarkBrush"] = new SolidColorBrush(Color.FromArgb("#4C1D95"));
                resources["SecondaryBrush"] = new SolidColorBrush(Color.FromArgb("#EDE9FE"));
                resources["PageBackgroundBrush"] = new SolidColorBrush(Color.FromArgb("#F4F0FF"));
                resources["CardBackgroundBrush"] = new SolidColorBrush(Color.FromArgb("#FFFFFF"));
                resources["InputBackgroundBrush"] = new SolidColorBrush(Color.FromArgb("#F5F3FF"));
                resources["TextPrimaryBrush"] = new SolidColorBrush(Color.FromArgb("#1E1B4B"));
                resources["TextSecondaryBrush"] = new SolidColorBrush(Color.FromArgb("#7C7598"));
                resources["TextMutedBrush"] = new SolidColorBrush(Color.FromArgb("#A39FBA"));
                resources["BorderSoftBrush"] = new SolidColorBrush(Color.FromArgb("#E9D5FF"));
            }
        }
    }

    public class TransactionDisplayItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string DateFormatted { get; set; } = string.Empty;
        public string AmountFormatted { get; set; } = string.Empty;
        public Color AmountColor { get; set; } = Colors.Black;
        public string CategoryIcon { get; set; } = "💳";
    }
}