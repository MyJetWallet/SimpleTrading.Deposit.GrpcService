using System.Threading.Tasks;
using SimpleTrading.Deposit.Grpc.Contracts;

namespace SimpleTrading.Deposit.GrpcService.Domains.Payment
{
    public interface IPaymentProvider<T>
    {
        public Task<T> CreatePaymentInvoice(CreatePaymentInvoiceGrpcRequest request,
            string traderId, string transactionId, string email);
    }
}