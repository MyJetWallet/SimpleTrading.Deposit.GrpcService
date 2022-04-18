using MyPostgreSQL;
using SimpleTrading.Deposit.GrcpService.Psql.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTrading.Deposit.GrcpService.Psql.Repositories
{
    public class PaymentSystemSettingsRepository: IPaymentSystemSettingsRepository
    {
        private readonly IPostgresConnection _postgresConnection;

        public PaymentSystemSettingsRepository(IPostgresConnection postgresConnection)
        {
            _postgresConnection = postgresConnection;
        }

        public async Task<IReadOnlyList<PaymentSystemSettingsEntity>> GetAllAsync()
        {
            const string sql = "select * from public.paymentsystemsettings_view";
            var entities = await _postgresConnection.GetRecordsAsync<PaymentSystemSettingsEntity>(sql);
            return entities.ToList();
        }
    }
}
