using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCrm.AuditLog.Grpc.Models;
using Newtonsoft.Json;
using SimpleTrading.Abstraction.Trading;
using SimpleTrading.Auth.Grpc.Contracts;
using SimpleTrading.ConvertService.Grpc.Contracts;
using SimpleTrading.Deposit.GrcpService.Psql.Models;
using SimpleTrading.Deposit.Grpc;
using SimpleTrading.Deposit.Grpc.Contracts;
using SimpleTrading.Deposit.GrpcService.Utils;
using SimpleTrading.Payments.Abstractions;

namespace SimpleTrading.Deposit.GrpcService
{
    public class DepositManagerApi : IDepositManagerGrpcService
    {
        public async ValueTask<CreatePaymentInvoiceGrpcResponse> CreatePaymentInvoiceAsync(
            CreatePaymentInvoiceGrpcRequest request)
        {
            var paymentProviderName =
                ServiceLocator.PaymentManager.GetPaymentProviderByPaymentSystem(request.PaymentSystemId);
            var brand = await GetBrandAsync(request.TraderId);
            var depositModel = request.ToDepositModel(paymentProviderName, brand);
            await ServiceLocator.DatabaseDepositRepository.Add(depositModel);
            await ServiceLocator.DepositCreatePublisher.PublishAsync(depositModel.ToDepositCreateServiceBusContract());
            depositModel.LogInvoiceByDepositModel();

            var authResponse = await ServiceLocator.AuthGrpcService.Value.GetEmailByIdAsync(new GetEmailByIdGrpcRequest
            {
                TraderId = request.TraderId
            });

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = request.TraderId,
                Action = "deposit",
                ActionId = depositModel.Id,
                DateTime = DateTime.UtcNow,
                Message = request.Comment ?? "Got deposit create request",
                Author = request.Author
            });

            return new CreatePaymentInvoiceGrpcResponse
            {
                Status = DepositManagerStatusEnum.Success,
                RedirectUrl = ServiceLocator.PaymentManager
                    .GetTexcentRedirectUrl(depositModel.Currency, depositModel.Id, depositModel.Amount,
                        authResponse.Email),
                TransactionId = depositModel.Id
            };
        }

        public async ValueTask<ProcessDepositResponse> ProcessDepositAsync(ProcessDepositRequest request)
        {
            ServiceLocator.Logger.Information("ProcessDepositAsync {@request}", 
                request);

            var unconfirmedInvoice =
                await ServiceLocator.DatabaseDepositRepository.FindPendingById(request.TransactionId)
                ?? (await ServiceLocator.DatabaseDepositRepository.FindPendingByPsId(request.PsTransactionId))
                .FirstOrDefault();

            if (unconfirmedInvoice == null)
            {
                ServiceLocator.Logger.Information("Invoice not found. Id: {id}, PsId: {psId}", request.TransactionId,
                    request.PsTransactionId);
                return new ProcessDepositResponse {Status = DepositManagerStatusEnum.ClientNotFound};
            }

            ServiceLocator.Logger.Information("ProcessDepositAsync {@invoice}",
                unconfirmedInvoice);

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = unconfirmedInvoice.TraderId,
                Action = "deposit",
                ActionId = request.TransactionId,
                DateTime = DateTime.UtcNow,
                Message = request.Comment ?? $"Handled deposit callback. Status: {request.PaymentInvoiceStatus}",
                Author = request.Author
            });

            if (request.PaymentInvoiceStatus != Payments.Abstractions.PaymentInvoiceStatusEnum.Approved)
            {
                await ServiceLocator.DatabaseDepositRepository.MakeFailed(request.TransactionId, request.PsTransactionId);
                await ServiceLocator.DepositUpdateStatusPublisher.PublishAsync(unconfirmedInvoice.ToDepositStatusUpdateServiceBusContract(unconfirmedInvoice.Status.ToString(), PaymentInvoiceStatusEnum.Failed.ToString()));
                return new ProcessDepositResponse {Status = DepositManagerStatusEnum.Error};
            }

            if (unconfirmedInvoice.AccountId.IsLiveMt())
            {
                ServiceLocator.Logger.Information("Try to send {mt5account} {amount}",
                    unconfirmedInvoice.AccountId, unconfirmedInvoice.Amount);
                try
                {
                    await unconfirmedInvoice.UpdateMtAccountBalanceOnCallback();
                    await ServiceLocator.DatabaseDepositRepository.MakeSuccess(request.TransactionId,
                        request.PsTransactionId);
                    await ServiceLocator.DepositUpdateStatusPublisher.PublishAsync(unconfirmedInvoice.ToDepositStatusUpdateServiceBusContract(unconfirmedInvoice.Status.ToString(), PaymentInvoiceStatusEnum.Approved.ToString()));
                    return new ProcessDepositResponse
                    {
                        Status = DepositManagerStatusEnum.Success
                    };
                }
                catch (Exception e)
                {
                    ServiceLocator.Logger.Fatal("Can't update MT5 account. {@invoice}", unconfirmedInvoice);
                    return new ProcessDepositResponse
                    {
                        Status = DepositManagerStatusEnum.Error
                    };
                }
            }

            // St  accounts
            var updateBalanceResponse = await unconfirmedInvoice.UpdateEngineAccountBalanceOnCallback();
            await ServiceLocator.DatabaseDepositRepository.MakeSuccess(request.TransactionId, request.PsTransactionId);
            await ServiceLocator.DepositUpdateStatusPublisher.PublishAsync(unconfirmedInvoice.ToDepositStatusUpdateServiceBusContract(unconfirmedInvoice.Status.ToString(), PaymentInvoiceStatusEnum.Approved.ToString()));
            var resultStatus = updateBalanceResponse.Result == TradingOperationResult.Ok
                ? DepositManagerStatusEnum.Success
                : DepositManagerStatusEnum.Error;

            return new ProcessDepositResponse {Status = resultStatus};
        }

        public async ValueTask<ProcessDepositResponse> HandleCryptoDepositCallback(
            HandleCryptoDepositCallbackRequest request)
        {
            request.LogCallbackRequest();

            var convertResponse = await ServiceLocator
                .ConvertService.Value.Convert(request.CreateConvertRequest());

            ServiceLocator.Logger.Information("Convert response: {data}",
                JsonConvert.SerializeObject(convertResponse));

            if (convertResponse.Status != ResponseStatuses.Success)
                return new ProcessDepositResponse { Status = DepositManagerStatusEnum.Error };
            var brand = await GetBrandAsync(request.TraderId);
            var depositModel = request.ToDepositModel(convertResponse, request.AccountId, brand);

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = depositModel.TraderId,
                Action = "deposit",
                ActionId = request.PsTransactionId,
                DateTime = DateTime.UtcNow,
                Message = $"Handled crypto deposit callback. Status: {request.Status}",
                Author = request.TraderId
            });

            await ServiceLocator.DatabaseDepositRepository.Add(depositModel);

            await ServiceLocator.DepositCreatePublisher.PublishAsync(depositModel.ToDepositCreateServiceBusContract());

            if (request.AccountId.IsLiveMt())
            {
                ServiceLocator.Logger.Information("Try to send {mt5account} {amount}",
                    request.AccountId, request.Amount);
                try
                {
                    await depositModel.UpdateMtAccountBalanceOnCallback();

                    await ServiceLocator.DepositUpdateStatusPublisher.PublishAsync(
                        depositModel.ToDepositStatusUpdateServiceBusContract(
                            depositModel.Status.ToString(), PaymentInvoiceStatusEnum.Approved.ToString()));

                    return new ProcessDepositResponse
                    {
                        Status = DepositManagerStatusEnum.Success
                    };
                }
                catch (Exception e)
                {
                    ServiceLocator.Logger.Fatal("Can't update MT5 account. {@deposit}", depositModel);
                    await ServiceLocator.DepositUpdateStatusPublisher.PublishAsync(
                        depositModel.ToDepositStatusUpdateServiceBusContract(
                            depositModel.Status.ToString(), PaymentInvoiceStatusEnum.Failed.ToString()));
                    return new ProcessDepositResponse
                    {
                        Status = DepositManagerStatusEnum.Error
                    };
                }
            }

            // St contracts
            var updateBalanceResponse = await depositModel.UpdateEngineAccountBalanceOnCallback();

            var resultStatus = updateBalanceResponse.Result == TradingOperationResult.Ok
                ? DepositManagerStatusEnum.Success
                : DepositManagerStatusEnum.Error;

            await ServiceLocator.DepositUpdateStatusPublisher.PublishAsync(
                depositModel.ToDepositStatusUpdateServiceBusContract(
                    depositModel.Status.ToString(), PaymentInvoiceStatusEnum.Approved.ToString()));

            return ProcessDepositResponse.Create(resultStatus);
        }

        private static async Task<string> GetBrandAsync(string traderId)
        {
            string brand = string.Empty;
            try
            {
                brand = await
                ServiceLocator.AccountRepository.GetBrandAsync(traderId);
            }
            catch (Exception ex)
            {
                ServiceLocator.Logger.Error(ex, "GetBrandAsync failed fro traderId {traderId}", traderId);
            }

            return brand;
        }

        public async ValueTask<GetCryptoWalletAddressResponse> GetCryptoWalletForClient(
            GetCryptoWalletAddressRequest request)
        {
            WalletModel wallet = null;
            if (request.AccountId.IsLiveMt())
            {
                wallet = (await ServiceLocator.DatabaseWalletRepository
                    .GetMtTraderAccount(request.TraderId));
            }

            if (request.AccountId.IsLiveSt())
            {
                wallet = (await ServiceLocator.DatabaseWalletRepository
                    .GetStTraderAccount(request.TraderId));
            }

            return wallet == null
                ? request.CreateGetCryptoWalletResponse(DepositManagerStatusEnum.ClientNotFound)
                : request.CreateGetCryptoWalletResponse(DepositManagerStatusEnum.Success, wallet.Id);
        }

        public async ValueTask<GetTraderResponse> GetTraderByAccountId(
            GetTraderRequest request)
        {
            var account = await ServiceLocator.Mt5AccountsRepository.GetTraderAccount(request.AccountId);

            return GetTraderResponse.Create(account?.TraderId);
        }

        public ValueTask<GetPaymentMethodsResponse> GetPaymentMethodsAsync()
        {
            return new ValueTask<GetPaymentMethodsResponse>(new GetPaymentMethodsResponse
            {
                Status = DepositManagerStatusEnum.Success,
                PaymentSystems = new List<PaymentSystemsEntity>
                {
                    new PaymentSystemsEntity {PaymentSystemId = "BANK_CARDS", Name = "Cards"},
                    new PaymentSystemsEntity {PaymentSystemId = "BITCOIN", Name = "Bitcoin"}
                }
            });
        }

        public async ValueTask<GetPaymentSystemsResponse> GetPaymentSystemsAsync(GetPaymentSystemsRequest request)
        {
            var settings = await ServiceLocator.PaymentSystemSettingsService.GetPaymentSystemSettingsAsync(request.TraderId, request.Brand, request.CountryIso3);
            return new GetPaymentSystemsResponse
            {
                Status = DepositManagerStatusEnum.Success,
                PaymentSystems = settings
            };
        }
    }
}