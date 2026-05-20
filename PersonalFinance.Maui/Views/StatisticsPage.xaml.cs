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

    private async Task LoadStatisticsAsync()
    {
        var transactions = await _apiService.GetTransactions(_user.Id);

        if (transactions == null || transactions.Count == 0)
        {
            EmptyViewLayout.IsVisible = true;
            MainContentGrid.IsVisible = false;
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
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnOpenGraphicalAnalysisClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GraphicalAnalysisPage(_user));
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