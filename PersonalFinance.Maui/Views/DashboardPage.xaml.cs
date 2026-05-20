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

        public DashboardPage(User user)
        {
            InitializeComponent();
            _user = user;
            _apiService = new ApiService();
            
            WelcomeLabel.Text = $"Merhaba, {_user.FullName}! 👋";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
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
                    if (t.Type == "Gelir")
                        totalIncome += t.Amount;
                    else if (t.Type == "Gider")
                        totalExpense += t.Amount;
                }

                decimal totalBalance = totalIncome - totalExpense;

                // Update UI Labels
                TotalBalanceLabel.Text = $"{totalBalance:N2} ₺";
                TotalIncomeLabel.Text = $"{totalIncome:N2} ₺";
                TotalExpenseLabel.Text = $"{totalExpense:N2} ₺";

                if (transactions.Count == 0)
                {
                    EmptyViewLayout.IsVisible = true;
                    TransactionsListLayout.IsVisible = false;
                }
                else
                {
                    EmptyViewLayout.IsVisible = false;
                    TransactionsListLayout.IsVisible = true;

                    var displayList = transactions
                        .OrderByDescending(t => t.TransactionDate)
                        .Take(5)
                        .Select(t => new TransactionDisplayItem
                        {
                            Id = t.Id,
                            Title = t.Title,
                            Category = t.Category,
                            DateFormatted = t.TransactionDate.ToString("dd.MM.yyyy HH:mm"),
                            AmountFormatted = t.Type == "Gelir" ? $"+{t.Amount:N2} ₺" : $"-{t.Amount:N2} ₺",
                            AmountColor = t.Type == "Gelir" ? Color.FromArgb("#10B981") : Color.FromArgb("#EF4444"),
                            CategoryIcon = GetCategoryIcon(t.Category)
                        })
                        .ToList();

                    BindableLayout.SetItemsSource(TransactionsListLayout, displayList);
                }
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
            await Navigation.PushAsync(new AddTransactionPage(_user));
        }

        private async void OnAllTransactionsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TransactionListPage(_user));
        }

        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChangePasswordPage(_user));
        }

        private async void OnStatisticsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new StatisticsPage(_user));
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
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
