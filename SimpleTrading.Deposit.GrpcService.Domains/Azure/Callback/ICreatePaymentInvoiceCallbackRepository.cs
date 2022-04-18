using System.Threading.Tasks;

namespace SimpleTrading.Deposit.GrpcService.Domains.Azure.Callback
{
    public interface ICreatePaymentInvoiceCallbackRepository
    {
        Task SaveAsync(ICreatePaymentInvoiceCallbackEntity log);
    }
}