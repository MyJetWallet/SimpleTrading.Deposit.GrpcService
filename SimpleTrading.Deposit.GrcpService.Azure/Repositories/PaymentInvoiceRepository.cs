using System.Collections.Generic;
using System.Threading.Tasks;
using MyAzureTableStorage;
using SimpleTrading.Deposit.Azure.Entities;

namespace SimpleTrading.Deposit.Azure.Repositories
{
    public class PaymentInvoiceRepository
    {
        private readonly IAzureTableStorage<PaymentInvoiceEntity> _tableStorage;

        public PaymentInvoiceRepository(IAzureTableStorage<PaymentInvoiceEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }
        
        public async Task SaveAsync(PaymentInvoiceEntity log)
        {
            await _tableStorage.InsertAsync(log);
        }
        
        public async Task BulkInsert(IEnumerable<PaymentInvoiceEntity> logs)
        {
            foreach (var log in logs)
            {
                await _tableStorage.InsertAsync(log);
            }
        }
    }
}