using System;

namespace PersonalFinance.Maui.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Type { get; set; } = string.Empty; // "Gelir" veya "Gider"

        public string Category { get; set; } = string.Empty;

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public string? Description { get; set; }

        public bool IsSaving { get; set; }

        public int UserId { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }

    public static class TransactionTypes
    {
        public const string Income = "Gelir";
        public const string Expense = "Gider";
    }
}
