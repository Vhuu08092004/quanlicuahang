using Microsoft.EntityFrameworkCore;
using Quanlicuahang.Data;
using Quanlicuahang.Models;

namespace Quanlicuahang.Repositories
{
    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        Task<decimal> GetTotalPaidByOrderAsync(string orderId);
        Task<object> GetCashflowAsync(DateTime? fromDate, DateTime? toDate, int skip, int take);
        Task<decimal> GetTotalPaidByOrderExcludingAsync(string orderId, string excludePaymentId);
    }

    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<decimal> GetTotalPaidByOrderAsync(string orderId)
        {
            var total = await _dbSet
                .Where(p => !p.IsDeleted && p.OrderId == orderId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;
            return total;
        }

        public async Task<decimal> GetTotalPaidByOrderExcludingAsync(string orderId, string excludePaymentId)
        {
            var total = await _dbSet
                .Where(p => !p.IsDeleted && p.OrderId == orderId && p.Id != excludePaymentId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;
            return total;
        }

        public async Task<object> GetCashflowAsync(DateTime? fromDate, DateTime? toDate, int skip, int take)
        {
            skip = skip < 0 ? 0 : skip;
            take = take <= 0 ? 10 : take;

            var realToDate = toDate?.AddDays(1);

            var incomeQuery = _context.Payments
                .Where(p => !p.IsDeleted
                            && (fromDate == null || p.PaymentDate >= fromDate)
                            && (toDate == null || p.PaymentDate < realToDate))
                .GroupBy(p => p.PaymentDate.Date)
                .Select(g => new { Date = g.Key, TotalIncome = g.Sum(x => x.Amount) });

            var expenseQuery = _context.Returns
                .Where(r => !r.IsDeleted
                            && (fromDate == null || r.ReturnDate >= fromDate)
                            && (toDate == null || r.ReturnDate < realToDate))
                .GroupBy(r => r.ReturnDate.Date)
                .Select(g => new { Date = g.Key, TotalExpense = g.Sum(x => x.RefundAmount) });

            var incomes = await incomeQuery.AsNoTracking().ToListAsync();
            var expenses = await expenseQuery.AsNoTracking().ToListAsync();

            var daily = incomes
                .Union(expenses.Select(e => new { Date = e.Date, TotalIncome = 0m }))
                .GroupBy(x => x.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Income = incomes.FirstOrDefault(i => i.Date == g.Key)?.TotalIncome ?? 0,
                    Expense = expenses.FirstOrDefault(e => e.Date == g.Key)?.TotalExpense ?? 0,
                    Net = (incomes.FirstOrDefault(i => i.Date == g.Key)?.TotalIncome ?? 0)
                          - (expenses.FirstOrDefault(e => e.Date == g.Key)?.TotalExpense ?? 0)
                })
                .OrderBy(x => x.Date)
                .ToList();

            var totalIncome = incomes.Sum(x => x.TotalIncome);
            var totalExpense = expenses.Sum(x => x.TotalExpense);
            var net = totalIncome - totalExpense;

            var total = daily.Count;
            var data = daily.Skip(skip).Take(take).ToList();

            return new
            {
                summary = new { totalIncome, totalExpense, net },
                data,
                total
            };
        }
    }
}
