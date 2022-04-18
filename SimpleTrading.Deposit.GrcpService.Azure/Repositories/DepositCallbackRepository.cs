using System.Collections.Generic;
using System.Threading.Tasks;
using MyAzureTableStorage;
using SimpleTrading.Deposit.Azure.Entities;

namespace SimpleTrading.Deposit.Azure.Repositories
{
    public class DepositCallbackRepository
    {
        private readonly IAzureTableStorage<DepositCallbackEntity> _tableStorage;

        public DepositCallbackRepository(IAzureTableStorage<DepositCallbackEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task SaveAsync(DepositCallbackEntity log)
        {
            await _tableStorage.InsertAsync(log);
        }

        public async Task BulkInsert(IEnumerable<DepositCallbackEntity> logs)
        {
            foreach (var log in logs)
            {
                await _tableStorage.InsertAsync(log);
            }
        }
    }
}