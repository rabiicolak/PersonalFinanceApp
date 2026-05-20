using PersonalFinance.Maui.Models;
using PersonalFinance.Maui.Services;
using System;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;

namespace PersonalFinance.Maui.Views
{
    public partial class EditTransactionPage : ContentPage
    {
        private readonly Transaction _transaction;
        private readonly ApiService _apiService;

        public EditTransactionPage(Transaction transaction)
        {
            InitializeComponent();
            _transaction = transaction;
            _apiService = new ApiService();

            PopulateFields();
        }

        private void PopulateFields()
        {
            TitleEntry.Text = _transaction.Title;
            AmountEntry.Text = _transaction.Amount.ToString("0.00", CultureInfo.InvariantCulture);
            
            if (_transaction.Type == "Gelir")
                IncomeRadio.IsChecked = true;
            else if (_transaction.Type == "Gider")
                ExpenseRadio.IsChecked = true;

            CategoryPicker.SelectedItem = _transaction.Category;
            if (CategoryPicker.SelectedIndex == -1)
            {
                CategoryPicker.SelectedItem = "Diğer";
            }

            TransactionDatePicker.Date = _transaction.TransactionDate;
            IsSavingCheckBox.IsChecked = _transaction.IsSaving;
            DescriptionEditor.Text = _transaction.Description;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnUpdateClicked(object sender, EventArgs e)
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

            amountText = amountText.Replace(',', '.');
            if (!decimal.TryParse(amountText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                await DisplayAlert("Hata", "Lütfen geçerli ve 0'dan büyük bir tutar girin.", "Tamam");
                return;
            }

            // 4. Model Güncelleme
            _transaction.Title = title;
            _transaction.Amount = amount;
            _transaction.Type = IncomeRadio.IsChecked ? "Gelir" : "Gider";
            _transaction.Category = CategoryPicker.SelectedItem?.ToString() ?? "Diğer";
            _transaction.TransactionDate = TransactionDatePicker.Date;
            _transaction.Description = DescriptionEditor.Text?.Trim();
            _transaction.IsSaving = IsSavingCheckBox.IsChecked;
            _transaction.UpdatedBy = _transaction.CreatedBy;
            _transaction.UpdatedDate = DateTime.Now;

            try
            {
                // 5. API Çağrısı
                bool isSuccess = await _apiService.UpdateTransaction(_transaction.Id, _transaction);

                if (isSuccess)
                {
                    await DisplayAlert("Başarılı", "İşlem başarıyla güncellendi.", "Tamam");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Hata", "İşlem güncellenirken sunucu tarafında bir hata oluştu.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Bir hata oluştu: {ex.Message}", "Tamam");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Silme Onayı", "Bu işlemi silmek istediğinize emin misiniz?", "Evet", "Hayır");
            if (!confirm) return;

            try
            {
                bool isSuccess = await _apiService.DeleteTransaction(_transaction.Id);

                if (isSuccess)
                {
                    await DisplayAlert("Başarılı", "İşlem başarıyla silindi.", "Tamam");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Hata", "İşlem silinirken sunucu tarafında bir hata oluştu.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Bir hata oluştu: {ex.Message}", "Tamam");
            }
        }
    }
}
