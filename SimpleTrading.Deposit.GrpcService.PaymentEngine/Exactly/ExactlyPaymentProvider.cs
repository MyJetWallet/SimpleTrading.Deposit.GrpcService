using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using SimpleTrading.Deposit.Grpc.Contracts;
using SimpleTrading.Deposit.GrpcService.Domains.Payment;
using SimpleTrading.Deposit.GrpcService.PaymentEngine.Exactly.Models;

namespace SimpleTrading.Deposit.GrpcService.PaymentEngine.Exactly
{
    public class ExactlyPaymentProvider : IPaymentProvider<CreatePaymentInvoiceResponse>
    {
        private readonly string _apiHost;
        private readonly string _successPage;
        private readonly string _errorPage;
        private readonly string _bearerToken;
        private readonly string _callbackUrl;
        private Dictionary<string, string> _paySystemsMapper;
        private ILogger _logger;

        public ExactlyPaymentProvider(string apiHost, string successPage, string errorPage, string callbackUrl,
            string bearerToke)
        {
            _apiHost = apiHost;
            _successPage = successPage;
            _errorPage = errorPage;
            _callbackUrl = callbackUrl;
            _bearerToken = bearerToke;
        }

        public ExactlyPaymentProvider SetupLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public ExactlyPaymentProvider SetupPaymentSystemMapper(Dictionary<string, string> mapper)
        {
            _logger.Information("Setup exactly payment mapper: {mapper}",
                JsonConvert.SerializeObject(mapper));
            _paySystemsMapper = mapper;
            return this;
        }

        public async Task<CreatePaymentInvoiceResponse> CreatePaymentInvoice(CreatePaymentInvoiceGrpcRequest request, string traderId, string transactionId, string email)
        {
            _logger.Information(
                "Creating exactly payment request for trader:{traderId}. Transaction id: {transactionId}",
                traderId, transactionId);

            var requestData = new
            {
                reference_id = transactionId,
                amount = request.DepositSum,
                currency = request.Currency,
                email,
                pay_method = _paySystemsMapper[request.PaymentSystemId],
                return_success_url = _successPage,
                return_error_url = _errorPage,
                callback_url = _callbackUrl,
                description = "Monfex pay"
            };

            _logger.Information("Sending data {data}", JsonConvert.SerializeObject(requestData));

            var response = await _apiHost
                .AppendPathSegments("api", "charges")
                .WithOAuthBearerToken(_bearerToken)
                .PostJsonAsync(requestData);

            return await response.ResponseMessage.ProcessPaymentProviderResponse<CreatePaymentInvoiceResponse>();
        }
    }
}