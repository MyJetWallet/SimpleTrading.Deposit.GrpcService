using System;
using SimpleTrading.Deposit.GrcpService.Psql.Models;
using SimpleTrading.Deposit.Grpc.Contracts;

namespace SimpleTrading.Deposit.GrpcService.Domains.Payment
{
    public interface ICreatePaymentInvoiceResponse
    {
        DepositModel ToDepositModel(CreatePaymentInvoiceGrpcRequest invoice, string guid, string traderId, DateTime timestamp);
    }
}