using System;
using System.Globalization;

namespace SimpleTrading.Deposit.GrpcService.PaymentEngine.Texcent
{
    public class TexcentPaymentProvider
    {
        private readonly string _baseUrl;

        public TexcentPaymentProvider(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public string GetRedirectUrl(string currency, string guid, double amount, string email)
        {
            var url = _baseUrl
                .AppendGetParam("currency", currency.ToLower())
                .AppendGetParam("orderId", guid)
                .AppendGetParam("payerEmail", email)
                .AppendGetParam("phoneNumber", new Random().Next(10000, 60000000).ToString())
                .AppendGetParam("payerName", "FirstName%20LastName")
                .AppendGetParam("amount", amount.ToString(CultureInfo.InvariantCulture));

            return url;
        }
    }
}