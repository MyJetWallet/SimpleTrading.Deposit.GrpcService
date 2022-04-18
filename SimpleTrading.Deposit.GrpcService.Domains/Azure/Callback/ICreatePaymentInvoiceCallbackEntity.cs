
using SimpleTrading.Payments.Abstractions;

namespace SimpleTrading.Deposit.GrpcService.Domains.Azure.Callback
{
    public interface ICreatePaymentInvoiceCallbackEntity
    {
        string TransactionId { get; }

        string PaymentProviderId { get; set; }

        PaymentInvoiceStatusEnum PaymentInvoiceStatus { get; set; }

        string TraderId { get; set; }

        string Commission { get; set; }

        string CallbackBody { get; set; }
    }
}