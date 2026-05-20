using PersonalFinance.Maui.Models;
using PersonalFinance.Maui.Services;

namespace PersonalFinance.Maui.Views;

public partial class StatisticsPage : ContentPage
{
    private readonly User _user;
    private readonly ApiService _apiService = new();

    public StatisticsPage(User user)
    {
        InitializeComponent();
        _user = user;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadStatisticsAsync();
    }

    private bool _isNavigating = false;

    private async Task LoadStatisticsAsync()
    {
        var transactions = await _apiService.GetTransactions(_user.Id);

        if (transactions == null || transactions.Count == 0)
        {
            EmptyViewLayout.IsVisible = true;
            MainContentGrid.IsVisible = false;
            
            // Animation for empty view
            EmptyViewLayout.Opacity = 0;
            EmptyViewLayout.TranslationY = 15;
            await Task.WhenAll(
                EmptyViewLayout.FadeTo(1, 300, Easing.CubicOut),
                EmptyViewLayout.TranslateTo(0, 0, 300, Easing.CubicOut)
            );
            return;
        }

        EmptyViewLayout.IsVisible = false;
        MainContentGrid.IsVisible = true;

        var incomeTransactions = transactions
            .Where(t => string.Equals(t.Type, "Gelir", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var expenseTransactions = transactions
            .Where(t => string.Equals(t.Type, "Gider", StringComparison.OrdinalIgnoreCase))
            .ToList();

        decimal totalIncome = incomeTransactions.Sum(t => t.Amount);
        decimal totalExpense = expenseTransactions.Sum(t => t.Amount);
        decimal netBalance = totalIncome - totalExpense;

        TotalTransactionsLabel.Text = transactions.Count.ToString();
        SavingTransactionsLabel.Text = transactions.Count(t => t.IsSaving).ToString();

        TotalIncomeLabel.Text = FormatMoney(totalIncome);
        TotalExpenseLabel.Text = FormatMoney(-totalExpense);
        NetBalanceLabel.Text = FormatMoney(netBalance);

        var topCategory = expenseTransactions
            .GroupBy(t => string.IsNullOrWhiteSpace(t.Category) ? "Diğer" : t.Category)
            .Select(g => new
            {
                Category = g.Key,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .FirstOrDefault();

        TopCategoryLabel.Text = topCategory == null
            ? "Yok"
            : $"{topCategory.Category} (-{topCategory.Amount:N0} ₺)";

        var categoryItems = expenseTransactions
            .GroupBy(t => string.IsNullOrWhiteSpace(t.Category) ? "Diğer" : t.Category)
            .Select(g =>
            {
                decimal amount = g.Sum(t => t.Amount);
                double percentage = totalExpense > 0 ? (double)(amount / totalExpense) : 0;

                return new CategoryExpenseItem
                {
                    Category = g.Key,
                    CategoryIcon = GetCategoryIcon(g.Key),
                    AmountFormatted = $"-{amount:N2} ₺",
                    Percentage = percentage,
                    PercentageFormatted = $"%{percentage * 100:N1}"
                };
            })
            .OrderByDescending(x => x.Percentage)
            .ToList();

        CategoryExpensesCollectionView.ItemsSource = categoryItems;

        // Dynamic Financial Insights Calculation
        if (totalIncome == 0 && totalExpense == 0)
        {
            InsightCard.IsVisible = false;
        }
        else
        {
            InsightCard.IsVisible = true;
            string insightText = "";

            if (totalExpense > totalIncome)
            {
                insightText = "Bu dönemde giderleriniz gelirinizi aşmış görünüyor. Bütçenizi dengelemek için harcama kategorilerinizi kontrol edebilirsiniz. ";
            }
            else if (totalIncome > 0 && totalExpense <= totalIncome * 0.5m)
            {
                insightText = "Harika! Gelirinizin yarısından fazlasını tasarruf ettiniz veya bütçenizde tuttunuz. Finansal durumunuz oldukça güçlü. ";
            }
            else
            {
                insightText = "Geliriniz giderlerinizi karşılıyor, finansal durumunuz dengeli görünüyor. ";
            }

            if (topCategory != null && topCategory.Amount > 0)
            {
                insightText += $"En yüksek harcama yaptığınız kategori: {topCategory.Category}. Bu kategorideki harcamalarınızı optimize etmeyi düşünebilirsiniz.";
            }

            InsightTextLabel.Text = insightText;
        }

        // Animate content entrance
        MainContentGrid.Opacity = 0;
        MainContentGrid.TranslationY = 15;
        await Task.WhenAll(
            MainContentGrid.FadeTo(1, 350, Easing.CubicOut),
            MainContentGrid.TranslateTo(0, 0, 350, Easing.CubicOut)
        );
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;
        try
        {
            await Navigation.PopAsync();
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private async void OnOpenGraphicalAnalysisClicked(object sender, EventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;
        try
        {
            await Navigation.PushAsync(new GraphicalAnalysisPage(_user));
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private static string FormatMoney(decimal amount)
    {
        return $"{amount:N2} ₺";
    }

    private static string GetCategoryIcon(string category)
    {
        return category switch
        {
            "Market" => "🛒",
            "Maaş" => "💰",
            "Fatura" => "🧾",
            "Eğlence" => "🎮",
            "Ulaşım" => "🚗",
            _ => "💳"
        };
    }

    public class CategoryExpenseItem
    {
        public string Category { get; set; } = "";
        public string CategoryIcon { get; set; } = "";
        public string AmountFormatted { get; set; } = "";
        public double Percentage { get; set; }
        public string PercentageFormatted { get; set; } = "";
    }
}