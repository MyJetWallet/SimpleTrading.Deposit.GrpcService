using SimpleTrading.Deposit.GrcpService.Psql.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleTrading.Deposit.GrcpService.Psql.Repositories
{
    public interface IPaymentSystemSettingsRepository
    {
        Task<IReadOnlyList<PaymentSystemSettingsEntity>> GetAllAsync();
    }
}