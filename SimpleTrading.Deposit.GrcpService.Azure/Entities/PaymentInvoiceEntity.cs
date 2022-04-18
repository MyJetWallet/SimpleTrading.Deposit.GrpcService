using System;
using Microsoft.WindowsAzure.Storage.Table;
using SimpleTrading.Deposit.GrcpService.Psql.Models;
using SimpleTrading.Payments.Abstractions;

namespace SimpleTrading.Deposit.Azure.Entities
{
    public class PaymentInvoiceEntity : TableEntity, IPaymentInvoice
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
        public string PaymentSystem { get; set; }
        public string PsTransactionId { get; set; }
        public string PaymentProvider { get; set; }
        public string PsCurrency { get; set; }
        public double PsAmount { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
        public PaymentInvoiceStatusEnum Status { get; set; }
        public string TraderId { get; set; }
        public string AccountId { get; set; }
        public string AdminId { get; set; }
        public string Comment { get; set; }
        public string VoidTransactionId { get; set; }

        public static PaymentInvoiceEntity CreateByDepositModel(DepositModel invoice)
        {
            var log = new PaymentInvoiceEntity
            {
                PaymentSystem = invoice.PaymentSystem,
                PsTransactionId = invoice.PsTransactionId,
                PsCurrency = invoice.PsCurrency,
                PsAmount = invoice.PsAmount,
                PaymentProvider = invoice.PaymentProvider,
                Currency = invoice.Currency,
                Amount = Convert.ToDouble(invoice.Amount),
                Status = invoice.Status,
                TraderId = invoice.TraderId,
                AccountId = invoice.AccountId,
                AdminId = invoice.AdminId,
                Comment = invoice.Comment,
                VoidTransactionId = invoice.VoidTransactionId
            };

            return Create(log, invoice.Id, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        }

        public static PaymentInvoiceEntity Create(IPaymentInvoice src, string pk,
            string rowKey)
        {
            return new PaymentInvoiceEntity
            {
                PartitionKey = GeneratePartitionKey(pk),
                RowKey = GenerateRowKey(rowKey),
                PaymentSystem = src.PaymentSystem,
                PsTransactionId = src.PsTransactionId,
                PaymentProvider = src.PaymentProvider,
                PsCurrency = src.PsCurrency,
                PsAmount = src.PsAmount,
                Currency = src.Currency,
                Amount = src.Amount,
                Status = src.Status,
                TraderId = src.TraderId,
                AccountId = src.AccountId,
                AdminId = src.AdminId,
                Comment = src.Comment,
                VoidTransactionId = src.VoidTransactionId
            };
        }
    }
}