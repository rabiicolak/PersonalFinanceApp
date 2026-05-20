using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Api.Data;
using PersonalFinance.Api.Models;

namespace PersonalFinance.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _context.Transactions
                .ToListAsync();

            return Ok(transactions);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var transactions = await _context.Transactions
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return Ok(transactions);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Transaction transaction)
        {
            transaction.CreatedDate = DateTime.Now;

            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();

            return Ok("İşlem eklendi.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Transaction updatedTransaction)
        {
            var transaction = await _context.Transactions
                .FindAsync(id);

            if (transaction == null)
            {
                return NotFound("İşlem bulunamadı.");
            }

            transaction.Title = updatedTransaction.Title;
            transaction.Amount = updatedTransaction.Amount;
            transaction.Type = updatedTransaction.Type;
            transaction.Category = updatedTransaction.Category;
            transaction.TransactionDate = updatedTransaction.TransactionDate;
            transaction.Description = updatedTransaction.Description;
            transaction.IsSaving = updatedTransaction.IsSaving;

            transaction.UpdatedBy = updatedTransaction.UpdatedBy;
            transaction.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok("İşlem güncellendi.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _context.Transactions
                .FindAsync(id);

            if (transaction == null)
            {
                return NotFound("İşlem bulunamadı.");
            }

            _context.Transactions.Remove(transaction);

            await _context.SaveChangesAsync();

            return Ok("İşlem silindi.");
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter(
            string? category,
            string? type)
        {
            var query = _context.Transactions.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(x => x.Category == category);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(x => x.Type == type);
            }

            var result = await query.ToListAsync();

            return Ok(result);
        }
    }
}