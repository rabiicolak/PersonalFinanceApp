using PersonalFinance.Maui.Models;
using PersonalFinance.Maui.Services;
using Microsoft.Maui.Graphics;

namespace PersonalFinance.Maui.Views;

public partial class GraphicalAnalysisPage : ContentPage
{
    private readonly User _user;
    private readonly ApiService _apiService = new();
    private readonly DonutDrawable _donutDrawable = new();

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

            _donutDrawable.IncomePercent = 0;
            _donutDrawable.ExpensePercent = 0;
            DonutGraphicsView.Invalidate();
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

        TotalFlowLabel.Text = $"{totalFlow:N0} ₺";
        IncomePercentageLabel.Text = $"Gelir: %{incomePercent:N1}";
        ExpensePercentageLabel.Text = $"Gider: %{expensePercent:N1}";

        _donutDrawable.IncomePercent = incomePercent;
        _donutDrawable.ExpensePercent = expensePercent;

        DonutGraphicsView.Invalidate();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
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