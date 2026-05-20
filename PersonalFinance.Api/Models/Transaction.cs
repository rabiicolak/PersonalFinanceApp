using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinance.Api.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty;
        // Gelir veya Gider

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsSaving { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}