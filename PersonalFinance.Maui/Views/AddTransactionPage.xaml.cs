using PersonalFinance.Maui.Models;
using PersonalFinance.Maui.Services;
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace PersonalFinance.Maui.Views
{
    public partial class AddTransactionPage : ContentPage
    {
        private readonly User _user;
        private readonly ApiService _apiService;

        public AddTransactionPage(User user)
        {
            InitializeComponent();
            _user = user;
            _apiService = new ApiService();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            // 1. Validasyon - Gelir / Gider Türü Seçimi
            if (!IncomeRadio.IsChecked && !ExpenseRadio.IsChecked)
            {
                await DisplayAlert("Hata", "Lütfen işlem türünü (Gelir veya Gider) seçin.", "Tamam");
                return;
            }

            // 2. Validasyon - Başlık
            string title = TitleEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title))
            {
                await DisplayAlert("Hata", "Başlık alanı boş bırakılamaz.", "Tamam");
                return;
            }

            // 3. Validasyon - Tutar
            string amountText = AmountEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(amountText))
            {
                await DisplayAlert("Hata", "Tutar alanı boş bırakılamaz.", "Tamam");
                return;
            }

            // Ondalık ayırıcıyı nokta olacak şekilde standardize etme
            amountText = amountText.Replace(',', '.');
            if (!decimal.TryParse(amountText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                await DisplayAlert("Hata", "Lütfen geçerli ve 0'dan büyük bir tutar girin.", "Tamam");
                return;
            }

            // 4. Model Oluşturma
            var transaction = new Transaction
            {
                Title = title,
                Amount = amount,
                Type = IncomeRadio.IsChecked ? "Gelir" : "Gider",
                Category = CategoryPicker.SelectedItem?.ToString() ?? "Diğer",
                TransactionDate = TransactionDatePicker.Date,
                Description = DescriptionEditor.Text?.Trim(),
                IsSaving = IsSavingCheckBox.IsChecked,
                UserId = _user.Id,
                CreatedBy = _user.FullName,
                CreatedDate = DateTime.Now
            };

            try
            {
                // 5. API Çağrısı
                bool isSuccess = await _apiService.AddTransaction(transaction);

                if (isSuccess)
                {
                    await DisplayAlert("Başarılı", "İşlem başarıyla eklendi.", "Tamam");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Hata", "İşlem kaydedilirken sunucu tarafında bir hata oluştu.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Bir hata oluştu: {ex.Message}", "Tamam");
            }
        }
    }
}
