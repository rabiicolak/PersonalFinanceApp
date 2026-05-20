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
    public partial class TransactionListPage : ContentPage
    {
        private readonly User _user;
        private readonly ApiService _apiService;
        private List<Transaction> _allTransactions = new();

        public TransactionListPage(User user)
        {
            InitializeComponent();
            _user = user;
            _apiService = new ApiService();
            
            // Kategori seçici varsayılan olarak "Tümü" seçilsin
            CategoryFilterPicker.SelectedIndex = 0;
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
                _allTransactions = await _apiService.GetTransactions(_user.Id);
                ApplyFilters();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Veriler yüklenirken bir hata oluştu: " + ex.Message, "Tamam");
            }
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allTransactions == null) return;

            var filtered = _allTransactions.AsEnumerable();

            // 1. İşlem Türü Filtresi
            if (IncomeRadio.IsChecked)
            {
                filtered = filtered.Where(t => t.Type == "Gelir");
            }
            else if (ExpenseRadio.IsChecked)
            {
                filtered = filtered.Where(t => t.Type == "Gider");
            }

            // 2. Kategori Filtresi
            string selectedCategory = CategoryFilterPicker.SelectedItem?.ToString() ?? "Tümü";
            if (selectedCategory != "Tümü")
            {
                filtered = filtered.Where(t => t.Category.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Tarihe Göre Sıralama (En yeni üstte)
            var sortedList = filtered.OrderByDescending(t => t.TransactionDate).ToList();

            // 4. Boş Liste Kontrolü ve Ekran Güncelleme
            if (sortedList.Count == 0)
            {
                EmptyViewLayout.IsVisible = true;
                TransactionsCollectionView.IsVisible = false;
            }
            else
            {
                EmptyViewLayout.IsVisible = false;
                TransactionsCollectionView.IsVisible = true;

                // 5. Ekran için Formatlama (DashboardPage'deki model sınıfı kullanılıyor)
                var displayList = sortedList.Select(t => new TransactionDisplayItem
                {
                    Id = t.Id,
                    Title = t.Title,
                    Category = t.Category,
                    DateFormatted = t.TransactionDate.ToString("dd.MM.yyyy HH:mm"),
                    AmountFormatted = t.Type == "Gelir" ? $"+{t.Amount:N2} ₺" : $"-{t.Amount:N2} ₺",
                    AmountColor = t.Type == "Gelir" ? Color.FromArgb("#10B981") : Color.FromArgb("#EF4444"),
                    CategoryIcon = GetCategoryIcon(t.Category)
                }).ToList();

                TransactionsCollectionView.ItemsSource = displayList;
            }
        }

        private string GetCategoryIcon(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return "💳";

            return category.ToLower() switch
            {
                "maaş" or "maas" or "gelir" => "💰",
                "market" => "🛒",
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

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnTransactionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is TransactionDisplayItem selectedItem)
            {
                // Seçimi hemen temizle
                TransactionsCollectionView.SelectedItem = null;

                // İlgili tam Transaction nesnesini bul ve düzenleme ekranını aç
                var selectedTransaction = _allTransactions.FirstOrDefault(t => t.Id == selectedItem.Id);
                if (selectedTransaction != null)
                {
                    await Navigation.PushAsync(new EditTransactionPage(selectedTransaction));
                }
            }
        }
    }
}
