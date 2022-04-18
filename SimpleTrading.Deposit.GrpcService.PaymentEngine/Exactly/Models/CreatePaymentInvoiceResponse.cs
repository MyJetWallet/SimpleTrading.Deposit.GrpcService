using System;
using Newtonsoft.Json;
using SimpleTrading.Deposit.GrcpService.Psql.Models;
using SimpleTrading.Deposit.Grpc.Contracts;
using SimpleTrading.Deposit.GrpcService.Domains.Payment;
using PaymentInvoiceStatusEnum = SimpleTrading.Payments.Abstractions.PaymentInvoiceStatusEnum;

namespace SimpleTrading.Deposit.GrpcService.PaymentEngine.Exactly.Models
{
    public class CreatePaymentInvoiceResponse : ICreatePaymentInvoiceResponse
    {
        [JsonProperty("data")] public ExactlyCreateInvoiceResponseData RequestData { get; set; }

        [JsonProperty("meta")] public ExactlyCreateInvoiceResponseMeta MetaData { get; set; }
        
        public DepositModel ToDepositModel(CreatePaymentInvoiceGrpcRequest invoice, string guid, string traderId, DateTime timestamp)
        {
            return new DepositModel
            {
                Id = guid,
                PaymentSystem = invoice.PaymentSystemId,
                PaymentProvider = "Exactly",
                PsTransactionId = RequestData.Charge.PsId,
                PsCurrency = RequestData.Charge.Attributes.Currency,
                PsAmount = double.Parse(RequestData.Charge.Attributes.Amount),
                Currency = invoice.Currency,
                Amount = invoice.DepositSum,
                Status = PaymentInvoiceStatusEnum.Registered,
                TraderId = traderId,
                AccountId = null,
                AdminId = null,
                Comment = RequestData.Charge.Attributes.Description,
                VoidTransactionId = null,
                DateTime = timestamp,
                PlatformDateTime = timestamp
            };
        }
    }

    public class ExactlyCreateInvoiceResponseMeta
    {
        [JsonProperty("server_time")] public long ServerTime { get; set; }

        [JsonProperty("server_timezone")] public string ServerTimezone { get; set; }

        [JsonProperty("api_version")] public string ApiVersion { get; set; }
    }

    public class ExactlyCreateInvoiceResponseData
    {
        [JsonProperty("charge")] public ExactlyCreateInvoiceResponseCharge Charge { get; set; }

        [JsonProperty("links")] public ExactlyCreateInvoiceResponseLinks Links { get; set; }
    }

    public class ExactlyCreateInvoiceResponseLinks
    {
        [JsonProperty("redirect_uri")] public string RedirectUrl { get; set; }
    }

    public class ExactlyCreateInvoiceResponseCharge
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("id")] public string PsId { get; set; }

        [JsonProperty("attributes")] public ExactlyCreateInvoiceResponseAttributes Attributes { get; set; }
    }

    public class ExactlyCreateInvoiceResponseAttributes
    {
        [JsonProperty("livemode")] public string Livemod { get; set; }

        [JsonProperty("status")] public string Status { get; set; }

        [JsonProperty("amount")] public string Amount { get; set; }

        [JsonProperty("paid_amount")] public string PaidAmount { get; set; }

        [JsonProperty("fee")] public string Fee { get; set; }

        [JsonProperty("rolling")] public string Rolling { get; set; }

        [JsonProperty("total_amount")] public string TotalAmount { get; set; }

        [JsonProperty("currency")] public string Currency { get; set; }

        [JsonProperty("pay_method")] public string PayMethod { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("secure_auth")] public string SecureAuth { get; set; }

        [JsonProperty("source")] public ExactlyCreateInvoiceResponseSource Source { get; set; }

        [JsonProperty("failure")] public string Failure { get; set; }

        [JsonProperty("reference_id")] public string ReferenceId { get; set; }

        [JsonProperty("created_at")] public int CreatedAt { get; set; }

        [JsonProperty("updated_at")] public int UpdatedAt { get; set; }

        [JsonProperty("valid_till")] public int ValidTill { get; set; }
    }

    public class ExactlyCreateInvoiceResponseSource
    {
        [JsonProperty("email")] public string Email { get; set; }
    }
}