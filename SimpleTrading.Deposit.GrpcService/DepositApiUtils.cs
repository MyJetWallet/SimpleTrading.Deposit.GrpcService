using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleTrading.Payments.Abstractions;
using SimpleTrading.Abstraction.Trading.BalanceOperations;
using SimpleTrading.ConvertService.Grpc.Contracts;
using SimpleTrading.Deposit.Azure.Entities;
using SimpleTrading.Deposit.GrcpService.Psql.Models;
using SimpleTrading.Deposit.Grpc.Contracts;
using SimpleTrading.Engine.Grpc.Contracts;
using SimpleTrading.Payments.ServiceBus.Models;
using SimpleTrading.ServiceBus.Contracts;

namespace SimpleTrading.Deposit.GrpcService
{
    public static class DepositApiUtils
    {
        public static DepositModel ToDepositModel(this CreatePaymentInvoiceGrpcRequest request, string targetProvider, string brand)
        {
            return new DepositModel
            {
                Id = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", ""),
                PaymentSystem = request.PaymentSystemId,
                PaymentProvider = targetProvider,
                Currency = request.Currency.ToUpper(),
                Amount = request.DepositSum,
                Status = Payments.Abstractions.PaymentInvoiceStatusEnum.Registered,
                TraderId = request.TraderId,
                AccountId = request.AccountId,
                DateTime = DateTime.UtcNow,
                PsCurrency = request.PsCurrency ?? request.Currency,
                PsAmount = request.PsAmount > 0 ? request.PsAmount : request.DepositSum,
                Brand = brand.ParseBrandOrDefault(),
                Type = request.Type?.ToString()
            };
        }

        public static void LogInvoiceByDepositModel(this DepositModel depositModel)
        {
            var logEntity = PaymentInvoiceEntity.CreateByDepositModel(depositModel);
            ServiceLocator.DepositEntitiesLoggerQueue.Enqueue(logEntity);
        }

        public static async Task UpdateMtAccountBalanceOnCallback(
            this DepositModel registeredInvoice)
        {

            ServiceLocator.Logger.Information("Try to get MT5 login");

            var traderMt5Login =
                await ServiceLocator.Mt5AccountsRepository.GetTraderMt5Login(registeredInvoice.AccountId,
                    registeredInvoice.TraderId);

            var changeBalanceRequest = new MyCrm.Mt5.Grpc.Contracts.ChangeBalanceGrpcRequest
            {
                LoginId = traderMt5Login,
                Delta = registeredInvoice.Amount,
                Comment = $"{registeredInvoice.TransactionId}",
                ProcessId = registeredInvoice.TransactionId,
            };

            ServiceLocator.Logger.Information("Try to change MT5 login balance {@changeBalanceRequest}", 
                changeBalanceRequest);

            var changeBalanceResponse =
                await ServiceLocator.Mt5GrpcServiceLive.ChangeBalanceAsync(changeBalanceRequest);

            ServiceLocator.Logger.Information("MT5 login balance was changed {@changeBalanceResponse}", 
                changeBalanceRequest);

            var response = await ServiceLocator.Mt5AccountsRepository.UpdateOnCallback(
                registeredInvoice.TraderId,
                registeredInvoice.AccountId,
                changeBalanceResponse.Margin,
                changeBalanceResponse.MarginFree,
                changeBalanceResponse.Profit,
                changeBalanceResponse.Balance,
                changeBalanceResponse.Equity);


            ServiceLocator.Logger.Information("Change mt balance: {changeBalanceRequest}",
                JsonConvert.SerializeObject(changeBalanceRequest));
        }

        public static async Task<ChangeBalanceGrpcResponse> UpdateEngineAccountBalanceOnCallback(
            this DepositModel registeredInvoice)
        {
            var changeBalanceRequest = new ChangeBalanceGrpcRequest
            {
                TraderId = registeredInvoice.TraderId,
                AccountId = registeredInvoice.AccountId,
                Delta = registeredInvoice.Amount,
                Comment = "Deposit",
                ProcessId = Guid.NewGuid().ToString(),
                OperationType = BalanceUpdateOperationType.Deposit
            };

            return await ServiceLocator.EngineLiveGrpcApi.Value.ChangeBalanceAsync(changeBalanceRequest);
        }

        public static void LogCallbackRequest(this HandleCryptoDepositCallbackRequest request)
        {
            var logEntity = DepositCallbackEntity.Create(new DepositCallbackEntity
            {
                PaymentProviderId = request.PaymentProvider,
                PaymentInvoiceStatus = request.Status,
                TraderId = request.TraderId,
                Commission = request.Commission,
                CallbackBody = request.CallbackEntity
            }, request.PsTransactionId, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

            ServiceLocator.DepositCallbackEntitiesLogQueue.Enqueue(logEntity);
        }

        public static CovertRequest CreateConvertRequest(this HandleCryptoDepositCallbackRequest request)
        {
            return new CovertRequest
            {
                Amount = double.Parse(request.Amount),
                InstrumentId = "BTCUSD",
                ConvertType = ConvertTypes.BaseToQuote
            };
        }

        public static DepositModel ToDepositModel(this HandleCryptoDepositCallbackRequest request,
            CovertResponse response, string accountId, string brand)
        {
            var guid = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
            var date = DateTime.UtcNow;
            DateTime? accepted = request.Status == Payments.Abstractions.PaymentInvoiceStatusEnum.Approved? date : (DateTime?)null;
            return new DepositModel
            {
                Id = guid,
                PaymentSystem = request.Currency,
                PsTransactionId = request.PsTransactionId,
                PaymentProvider = request.PaymentProvider,
                PsCurrency = request.Currency,
                PsAmount = double.Parse(request.Amount),
                Currency = "USD",
                // 0.97 - 3% of deposit commission
                Amount = response.ConvertedAmount * 0.97,
                Status = request.Status,
                TraderId = request.TraderId,
                AccountId = accountId,
                AdminId = null,
                Comment = null,
                VoidTransactionId = null,
                DateTime = date,
                PlatformDateTime = date,
                Accepted = accepted,
                Brand = brand.ParseBrandOrDefault()
            };
        }

        public static GetCryptoWalletAddressResponse CreateGetCryptoWalletResponse(
            this GetCryptoWalletAddressRequest request,
            DepositManagerStatusEnum statusEnum, string wallet = null)
        {
            return new GetCryptoWalletAddressResponse
            {
                WalletAddress = wallet,
                Status = statusEnum
            };
        }

        public static DepositCreateServiceBusContract ToDepositCreateServiceBusContract(this DepositModel depositModel)
        {
            return new DepositCreateServiceBusContract
            {
                AccountId = depositModel.AccountId,
                Amount = (decimal) depositModel.Amount,
                BrandId = depositModel.Brand.ToString(),
                Currency = depositModel.Currency,
                TraderId = depositModel.TraderId,
                Transactionid = depositModel.TransactionId,
                Status = depositModel.Status.ToString(),
                PaymentSystem = depositModel.PaymentSystem
            };
        }

        public static DepositStatusUpdateServiceBusContract ToDepositStatusUpdateServiceBusContract(this DepositModel depositModel, string oldStatus, string newStatus)
        {
            return new DepositStatusUpdateServiceBusContract
            {
                AccountId = depositModel.AccountId,
                Amount = (decimal) depositModel.Amount,
                BrandId = depositModel.Brand.ToString(),
                Currency = depositModel.Currency,
                TraderId = depositModel.TraderId,
                Transactionid = depositModel.TransactionId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                PaymentSystem = depositModel.PaymentSystem
            };
        }

        private static Postgresql.Models.BrandName ParseBrandOrDefault(this string brand)
        {
            Enum.TryParse<Postgresql.Models.BrandName>(brand, true, out var result);
            return result;
        }
    }
}