using PersonalFinance.Maui.Models;
using PersonalFinance.Maui.Services;
using Microsoft.Maui.Graphics;

namespace PersonalFinance.Maui.Views;

public partial class GraphicalAnalysisPage : ContentPage
{
    private readonly User _user;
    private readonly ApiService _apiService = new();
    private readonly DonutDrawable _donutDrawable = new();
    private bool _isNavigating = false;

    public GraphicalAnalysisPage(User user)
    {
        InitializeComponent();
        _user = user;
        DonutGraphicsView.Drawable = _donutDrawable;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadGraphicalDataAsync();
    }

    private async Task LoadGraphicalDataAsync()
    {
        var transactions = await _apiService.GetTransactions(_user.Id);

        if (transactions == null || transactions.Count == 0)
        {
            TotalFlowLabel.Text = "0 ₺";
            IncomePercentageLabel.Text = "Gelir: %0";
            ExpensePercentageLabel.Text = "Gider: %0";
            IncomeRatioLabel.Text = "%0";
            ExpenseRatioLabel.Text = "%0";
            TotalVolumeLabel.Text = "0,00 ₺";

            _donutDrawable.IncomePercent = 0;
            _donutDrawable.ExpensePercent = 0;
            DonutGraphicsView.Invalidate();

            MonthlyAnalysisCollectionView.ItemsSource = new List<MonthlyAnalysisItem>();

            MainContentLayout.Opacity = 0;
            MainContentLayout.TranslationY = 15;

            await Task.WhenAll(
                MainContentLayout.FadeTo(1, 300, Easing.CubicOut),
                MainContentLayout.TranslateTo(0, 0, 300, Easing.CubicOut)
            );

            return;
        }

        var totalIncome = transactions
            .Where(t => string.Equals(t.Type, "Gelir", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);

        var totalExpense = transactions
            .Where(t => string.Equals(t.Type, "Gider", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);

        var totalFlow = totalIncome + totalExpense;

        var incomePercent = totalFlow > 0
            ? (double)(totalIncome / totalFlow) * 100
            : 0;

        var expensePercent = totalFlow > 0
            ? (double)(totalExpense / totalFlow) * 100
            : 0;

        TotalFlowLabel.Text = $"{totalFlow:N2} ₺";
        IncomePercentageLabel.Text = $"Gelir: %{incomePercent:N1}";
        ExpensePercentageLabel.Text = $"Gider: %{expensePercent:N1}";

        IncomeRatioLabel.Text = $"%{incomePercent:N1}";
        ExpenseRatioLabel.Text = $"%{expensePercent:N1}";
        TotalVolumeLabel.Text = $"{totalFlow:N2} ₺";

        _donutDrawable.IncomePercent = incomePercent;
        _donutDrawable.ExpensePercent = expensePercent;
        DonutGraphicsView.Invalidate();

        MonthlyAnalysisCollectionView.ItemsSource = GetMonthlyAnalysis(transactions);

        MainContentLayout.Opacity = 0;
        MainContentLayout.TranslationY = 15;

        await Task.WhenAll(
            MainContentLayout.FadeTo(1, 350, Easing.CubicOut),
            MainContentLayout.TranslateTo(0, 0, 350, Easing.CubicOut)
        );
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (_isNavigating)
            return;

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

    private static readonly string[] TurkishMonths =
    {
        "", "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
        "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"
    };

    private List<MonthlyAnalysisItem> GetMonthlyAnalysis(List<Transaction> transactions)
    {
        return transactions
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g =>
            {
                var year = g.Key.Year;
                var month = g.Key.Month;
                var monthName = month >= 1 && month <= 12 ? TurkishMonths[month] : "";
                var monthYearName = $"{monthName} {year}";

                var totalIncome = g
                    .Where(t => string.Equals(t.Type, "Gelir", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Amount);

                var totalExpense = g
                    .Where(t => string.Equals(t.Type, "Gider", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Amount);

                var netBalance = totalIncome - totalExpense;
                var totalFlow = totalIncome + totalExpense;

                var incomeRatio = totalFlow > 0 ? (double)(totalIncome / totalFlow) : 0.5;
                var expenseRatio = totalFlow > 0 ? (double)(totalExpense / totalFlow) : 0.5;

                var maxAmount = Math.Max(totalIncome, totalExpense);

                var incomeBarHeight = totalIncome > 0 && maxAmount > 0
                    ? Math.Max(10, (double)(totalIncome / maxAmount) * 120)
                    : 4;

                var expenseBarHeight = totalExpense > 0 && maxAmount > 0
                    ? Math.Max(10, (double)(totalExpense / maxAmount) * 120)
                    : 4;

                return new MonthlyAnalysisItem
                {
                    MonthYearName = monthYearName,
                    TotalIncomeFormatted = $"+{totalIncome:N2} ₺",
                    TotalExpenseFormatted = $"-{totalExpense:N2} ₺",
                    NetBalanceFormatted = $"{(netBalance >= 0 ? "+" : "")}{netBalance:N2} ₺",
                    NetBalanceColor = netBalance >= 0
                        ? Color.FromArgb("#10B981")
                        : Color.FromArgb("#EF4444"),
                    IncomeRatio = incomeRatio,
                    ExpenseRatio = expenseRatio,
                    IncomeBarHeight = incomeBarHeight,
                    ExpenseBarHeight = expenseBarHeight,
                    Year = year,
                    Month = month
                };
            })
            .OrderByDescending(i => i.Year)
            .ThenByDescending(i => i.Month)
            .ToList();
    }

    public class DonutDrawable : IDrawable
    {
        public double IncomePercent { get; set; }
        public double ExpensePercent { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            float centerX = dirtyRect.Center.X;
            float centerY = dirtyRect.Center.Y;
            float radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f - 24f;
            float strokeWidth = 30f;

            canvas.StrokeSize = strokeWidth;
            canvas.StrokeLineCap = LineCap.Round;

            canvas.StrokeColor = Color.FromArgb("#E5E7EB");
            canvas.DrawCircle(centerX, centerY, radius);

            float incomeRatio = NormalizePercent(IncomePercent);
            float expenseRatio = NormalizePercent(ExpensePercent);

            float totalRatio = incomeRatio + expenseRatio;

            if (totalRatio <= 0)
                return;

            incomeRatio /= totalRatio;
            expenseRatio /= totalRatio;

            float startAngle = -90f;
            float incomeSweep = incomeRatio * 360f;
            float expenseSweep = expenseRatio * 360f;

            if (incomeSweep > 0)
            {
                canvas.StrokeColor = Color.FromArgb("#10B981");
                DrawArcWithLines(canvas, centerX, centerY, radius, startAngle, incomeSweep);
            }

            if (expenseSweep > 0)
            {
                canvas.StrokeColor = Color.FromArgb("#EF4444");
                DrawArcWithLines(canvas, centerX, centerY, radius, startAngle + incomeSweep, expenseSweep);
            }
        }

        private static float NormalizePercent(double value)
        {
            if (value > 1)
                return (float)Math.Clamp(value / 100.0, 0, 1);

            return (float)Math.Clamp(value, 0, 1);
        }

        private static void DrawArcWithLines(
            ICanvas canvas,
            float centerX,
            float centerY,
            float radius,
            float startAngle,
            float sweepAngle)
        {
            if (sweepAngle <= 0)
                return;

            float step = 1.5f;
            float endAngle = startAngle + sweepAngle;

            for (float angle = startAngle; angle < endAngle; angle += step)
            {
                float nextAngle = Math.Min(angle + step, endAngle);

                var p1 = PointOnCircle(centerX, centerY, radius, angle);
                var p2 = PointOnCircle(centerX, centerY, radius, nextAngle);

                canvas.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
            }
        }

        private static PointF PointOnCircle(
            float centerX,
            float centerY,
            float radius,
            float angleDegrees)
        {
            double radians = Math.PI * angleDegrees / 180.0;

            return new PointF(
                centerX + radius * (float)Math.Cos(radians),
                centerY + radius * (float)Math.Sin(radians)
            );
        }
    }
}

public class MonthlyAnalysisItem
{
    public string MonthYearName { get; set; } = string.Empty;
    public string TotalIncomeFormatted { get; set; } = string.Empty;
    public string TotalExpenseFormatted { get; set; } = string.Empty;
    public string NetBalanceFormatted { get; set; } = string.Empty;
    public Color NetBalanceColor { get; set; } = Colors.Black;
    public double IncomeRatio { get; set; }
    public double ExpenseRatio { get; set; }
    public double IncomeBarHeight { get; set; }
    public double ExpenseBarHeight { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }

    public GridLength IncomeColumnWidth => new GridLength(IncomeRatio, GridUnitType.Star);
    public GridLength ExpenseColumnWidth => new GridLength(ExpenseRatio, GridUnitType.Star);
}