using Microsoft.WindowsAzure.Storage.Table;
using SimpleTrading.Deposit.GrpcService.Domains.Azure.Callback;
using SimpleTrading.Payments.Abstractions;

namespace SimpleTrading.Deposit.Azure.Entities
{
    public class DepositCallbackEntity : TableEntity, ICreatePaymentInvoiceCallbackEntity
    {
        private static string GeneratePartitionKey(string id)
        {
            return id;
        }
        private static string GenerateRowKey(string row)
        {
            return row;
        }
        
        public string TransactionId => PartitionKey;
        public string PaymentProviderId { get; set; }
        public PaymentInvoiceStatusEnum PaymentInvoiceStatus { get; set; }
        public string TraderId { get; set; }
        public string Commission { get; set; }
        public string CallbackBody { get; set; }
        
        public static DepositCallbackEntity Create(ICreatePaymentInvoiceCallbackEntity model, string pk, string rk)
        {
            return new DepositCallbackEntity
            {
                PartitionKey = GeneratePartitionKey(pk),
                RowKey = GenerateRowKey(rk),
                PaymentProviderId = model.PaymentProviderId,
                PaymentInvoiceStatus = model.PaymentInvoiceStatus,
                TraderId = model.TraderId,
                Commission = model.Commission,
                CallbackBody = model.CallbackBody
            };
        }
    }
}