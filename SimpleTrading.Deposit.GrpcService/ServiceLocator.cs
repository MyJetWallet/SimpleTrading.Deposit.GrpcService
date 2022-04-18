using DotNetCoreDecorators;
using MyCrm.AuditLog.Grpc;
using MyCrm.Mt5.Grpc;
using MyDependencies;
using Serilog;
using Serilog.Core;
using SimpleTrading.Auth.Grpc;
using SimpleTrading.ConvertService.Grpc;
using SimpleTrading.Deposit.Azure.Entities;
using SimpleTrading.Deposit.Azure.Repositories;
using SimpleTrading.Deposit.GrcpService.Psql.Accounts;
using SimpleTrading.Deposit.GrcpService.Psql.Models;
using SimpleTrading.Deposit.GrcpService.Psql.Repositories;
using SimpleTrading.Deposit.GrpcService.PaymentEngine;
using SimpleTrading.Deposit.GrpcService.Services;
using SimpleTrading.Engine.Grpc;
using SimpleTrading.GrpcTemplate;
using SimpleTrading.ServiceBus.Contracts;
using System.Collections.Generic;
using SimpleTrading.Payments.ServiceBus.Models;

namespace SimpleTrading.Deposit.GrpcService
{
    public static class ServiceLocator
    {
        public static PaymentManager PaymentManager { get; private set; }
        public static DepositRepository DatabaseDepositRepository { get; private set; }
        public static IAccountRepository AccountRepository { get; private set; }
        public static ILogger Logger { get; private set; }
        public static BaseQueue<PaymentInvoiceEntity> DepositEntitiesLoggerQueue { get; private set; }
        public static BaseQueue<DepositCallbackEntity> DepositCallbackEntitiesLogQueue { get; private set; }
        public static GrpcServiceClient<ISimpleTradingEngineApi> EngineLiveGrpcApi { get; private set; }
        public static WalletRepository DatabaseWalletRepository { get; private set; }
        public static GrpcServiceClient<IConvertService> ConvertService { get; private set; }
        public static PaymentInvoiceRepository InvoiceRepository { get; private set; }
        public static DepositCallbackRepository CallbackRepository { get; private set; }
        public static IMyCrmMt5GrpcService Mt5GrpcServiceLive { get; private set; }
        public static Mt5AccountsRepository Mt5AccountsRepository { get; private set; }
        public static GrpcServiceClient<IAuthGrpcService> AuthGrpcService { get; private set; }
        public static IMyCrmAuditLogGrpcService AuditLogGrpcService { get; private set; }
        public static IPublisher<DepositCreateServiceBusContract> DepositCreatePublisher { get; private set; }
        public static IPublisher<DepositStatusUpdateServiceBusContract> DepositUpdateStatusPublisher { get; private set; }
        public static IPaymentSystemSettingsService PaymentSystemSettingsService { get; private set; }
        public static IPaymentSystemSettingsRepository PaymentSystemSettingsRepository { get; private set; }
        public static ILocalCache<IPaymentSystemSettingsService, string, IReadOnlyList<PaymentSystemSettingsEntity>> PaymentSystemSettingsEntityCache { get; private set; }


        public static void Init(IServiceResolver sr)
        {
            PaymentManager = sr.GetService<PaymentManager>();
            InvoiceRepository = sr.GetService<PaymentInvoiceRepository>();
            CallbackRepository = sr.GetService<DepositCallbackRepository>();
            DatabaseDepositRepository = sr.GetService<DepositRepository>();
            AccountRepository = sr.GetService<IAccountRepository>();
            AuthGrpcService = sr.GetService<GrpcServiceClient<IAuthGrpcService>>();
            DatabaseWalletRepository = sr.GetService<WalletRepository>();
            EngineLiveGrpcApi = sr.GetService<GrpcServiceClient<ISimpleTradingEngineApi>>();
            ConvertService = sr.GetService<GrpcServiceClient<IConvertService>>();
            DepositEntitiesLoggerQueue = sr.GetService<BaseQueue<PaymentInvoiceEntity>>();
            DepositCallbackEntitiesLogQueue = sr.GetService<BaseQueue<DepositCallbackEntity>>();
            Mt5GrpcServiceLive = sr.GetService<IMyCrmMt5GrpcService>();
            Mt5AccountsRepository = sr.GetService<Mt5AccountsRepository>();
            AuditLogGrpcService = sr.GetService<IMyCrmAuditLogGrpcService>();
            Logger = sr.GetService<ILogger>();
            DepositCreatePublisher = sr.GetService<IPublisher<DepositCreateServiceBusContract>>();
            DepositUpdateStatusPublisher = sr.GetService<IPublisher<DepositStatusUpdateServiceBusContract>>();
            PaymentSystemSettingsService = sr.GetService<IPaymentSystemSettingsService>();
            PaymentSystemSettingsRepository = sr.GetService<IPaymentSystemSettingsRepository>();
            PaymentSystemSettingsEntityCache = sr.GetService<ILocalCache<IPaymentSystemSettingsService, string, IReadOnlyList<PaymentSystemSettingsEntity>>>();
        }
    }
}