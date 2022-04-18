using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleTrading.Payments.Abstractions;

namespace SimpleTrading.Deposit.GrpcService.Domains.Azure.Request
{
    public interface IPaymentRepository
    {
        Task SaveAsync(IPaymentInvoice log);
        Task BulkInsertAsync(IEnumerable<IPaymentInvoice> log);
    }
}