using System;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using SimpleTrading.Deposit.Grpc.Contracts;
using SimpleTrading.Deposit.GrpcService.PaymentEngine.Exactly;
using SimpleTrading.Deposit.GrpcService.PaymentEngine.Exactly.Models;
using SimpleTrading.Deposit.GrpcService.PaymentEngine.Texcent;

namespace SimpleTrading.Deposit.GrpcService.PaymentEngine
{
    public class PaymentManager
    {
        private ExactlyPaymentProvider _exactlyPaymentProvider;

        private TexcentPaymentProvider _texcentPaymentProvider;

        private ILogger _logger;

        public PaymentManager SetupLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public async Task<CreatePaymentInvoiceResponse> CreateExactlyPaymentInvoice(
            CreatePaymentInvoiceGrpcRequest request,
            string traderId, string transactionId, string email)
        {
            _logger.Information("Creating payment invoice for trader: {traderId}", traderId);
            if (request.PaymentSystemId != "BANK_CARDS")
            {
                _logger.Fatal("Payment system not found. System: {system}", request.PaymentSystemId);
                throw new HttpRequestException("Payment system not found");
            }

            var exactlyResponse =
                await _exactlyPaymentProvider.CreatePaymentInvoice(request, traderId, transactionId, email);
            return exactlyResponse;
        }

        public string GetPaymentProviderByPaymentSystem(string paymentSystem)
        {
            return paymentSystem switch
            {
                "WireTransfer" => "WireTransfer",
                "BitcoinCoinPayments" => "BitcoinCoinPayments",
                _ => "Texcent"
            };
        }
        
        public string GetTexcentRedirectUrl(string currency, string guid, double amount, string email)
        {
            return _texcentPaymentProvider.GetRedirectUrl(currency, guid, amount, email);
        }

        public PaymentManager SetupExactlyPaymentSystem(Func<ExactlyPaymentProvider> func)
        {
            _exactlyPaymentProvider = func();
            return this;
        }

        public PaymentManager SetupTexcentPaymentSystem(Func<TexcentPaymentProvider> func)
        {
            _texcentPaymentProvider = func();
            return this;
        }
    }
}