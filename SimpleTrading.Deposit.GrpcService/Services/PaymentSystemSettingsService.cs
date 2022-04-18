using SimpleTrading.Deposit.GrcpService.Psql.Models;
using SimpleTrading.Deposit.GrcpService.Psql.Repositories;
using SimpleTrading.Deposit.Grpc.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTrading.Deposit.GrpcService.Services
{
    public class PaymentSystemSettingsService: IPaymentSystemSettingsService
    {
        private IPaymentSystemSettingsRepository PaymentSystemSettings => ServiceLocator.PaymentSystemSettingsRepository;
        private ILocalCache<IPaymentSystemSettingsService, string, IReadOnlyList<PaymentSystemSettingsEntity>> PaymentSystemSettingsCache => ServiceLocator.PaymentSystemSettingsEntityCache;

        public async ValueTask<IReadOnlyCollection<PaymentSystemsEntity>> GetPaymentSystemSettingsAsync(string traderId, string brand, string country)
        {
            var result = await LoadAsync(traderId, brand, country);

            return result;
        }

        private async ValueTask<IReadOnlyCollection<PaymentSystemsEntity>> LoadAsync(string traderId, string brand, string country)
        {
            IReadOnlyList<PaymentSystemSettingsEntity> paymentSystemSettingsEntities = await PaymentSystemSettingsCache.GetOrAddAsync(nameof(PaymentSystemSettingsEntity), () => PaymentSystemSettings.GetAllAsync());
            var allowedSettings = paymentSystemSettingsEntities.Where(x => x.IsEnable
            && x.Brand.Equals(brand, System.StringComparison.OrdinalIgnoreCase)
            && x.IsSupportCountry(country)
            && x.IsNotRestrictedCountry(country));
            var result = allowedSettings.Select(x => new PaymentSystemsEntity()
            {
                Name = x.PaymentSystemName,
                PaymentSystemId = x.PaymentSystemName
            }).ToList();

            return result;
        }
    }
}
