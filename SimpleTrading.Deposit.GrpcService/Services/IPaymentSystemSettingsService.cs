using SimpleTrading.Deposit.Grpc.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTrading.Deposit.GrpcService.Services
{
    public interface IPaymentSystemSettingsService
    {
        ValueTask<IReadOnlyCollection<PaymentSystemsEntity>> GetPaymentSystemSettingsAsync(string traderId, string brand, string country);
    }
}