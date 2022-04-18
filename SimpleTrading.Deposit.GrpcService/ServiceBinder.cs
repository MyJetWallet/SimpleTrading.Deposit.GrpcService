using System.Collections.Generic;
using DotNetCoreDecorators;
using Grpc.Net.Client;
using MyCrm.AuditLog.Grpc;
using MyCrm.Mt5.Grpc;
using MyDependencies;
using MyPostgreSQL;
using MyServiceBus.TcpClient;
using ProtoBuf.Grpc.Client;
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
using SimpleTrading.Deposit.GrpcService.PaymentEngine.Exactly;
using SimpleTrading.Deposit.GrpcService.PaymentEngine.Texcent;
using SimpleTrading.Deposit.GrpcService.Services;
using SimpleTrading.Engine.Grpc;
using SimpleTrading.GrpcTemplate;
using SimpleTrading.Payments.ServiceBus.Deposit;
using SimpleTrading.Payments.ServiceBus.Models;
using SimpleTrading.ServiceBus.Contracts;


namespace SimpleTrading.Deposit.GrpcService
{
    public static class ServiceBinder
    {
        private const string AppName = "DepositGrpcManager";

        public static void BindAzureTables(this IServiceRegistrator sr, SettingModel settingModel)
        {
            var createInvoiceRequestLogTable =
                new MyAzureTableStorage.AzureTableStorage<PaymentInvoiceEntity>(
                    () => settingModel.AzureStorageConnString, settingModel.AzureCreateInvoiceRequestLogTable);

            var createInvoiceCallbackLogTable =
                new MyAzureTableStorage.AzureTableStorage<DepositCallbackEntity>(
                    () => settingModel.AzureStorageConnString, settingModel.AzureCreateInvoiceCallbackLogTable);

            var paymentInvoiceRepository = new PaymentInvoiceRepository(createInvoiceRequestLogTable);
            var depositCallbackRepository = new DepositCallbackRepository(createInvoiceCallbackLogTable);

            sr.Register(paymentInvoiceRepository);
            sr.Register(depositCallbackRepository);
        }

        public static void BindPaymentManager(this IServiceRegistrator sr, SettingModel settingModel, ILogger logger)
        {
            var exactlyPaymentSystem = new ExactlyPaymentProvider(settingModel.ExactlyApiHost, settingModel.SuccessPage,
                    settingModel.FailPage, settingModel.CallbackUrl, settingModel.ExactlyBearerToken)
                .SetupLogger(logger)
                .SetupPaymentSystemMapper(new Dictionary<string, string> {{"BANK_CARDS", "card"}});

            var paymentManager = new PaymentManager()
                .SetupLogger(logger)
                .SetupExactlyPaymentSystem(() => exactlyPaymentSystem)
                .SetupTexcentPaymentSystem(() => new TexcentPaymentProvider(settingModel.TexcentBaseUrl));

            sr.Register(paymentManager);
        }

        public static void BindGrpcServices(this IServiceRegistrator sr,
            SettingModel settingModel)
        {
            sr.Register(new GrpcServiceClient<ISimpleTradingEngineApi>(
                () => settingModel.EngineLiveGrpcServerUrl));

            sr.Register(new GrpcServiceClient<IConvertService>(
                () => settingModel.ConvertGrpcServerUrl));

            sr.Register(new GrpcServiceClient<IAuthGrpcService>(
                () => settingModel.AuthGrpcServiceUrl));

            sr.Register(GrpcChannel.ForAddress(settingModel.MtBridgeLive)
                .CreateGrpcService<IMyCrmMt5GrpcService>());
            
            sr.Register(GrpcChannel.ForAddress(settingModel.AuditLogGrpcService)
                .CreateGrpcService<IMyCrmAuditLogGrpcService>());
        }

        public static void BindDatabaseDepositRepository(this IServiceRegistrator sr, SettingModel settingModel)
        {
            sr.Register(new DepositRepository(settingModel.DatabaseConnString, AppName));
            sr.Register(new WalletRepository(new PostgresConnection(settingModel.DatabaseConnString, AppName)));
            sr.Register(new Mt5AccountsRepository(new PostgresConnection(settingModel.DatabaseConnString, AppName)));
            sr.Register<IAccountRepository>(new AccountRepository(new PostgresConnection(settingModel.DatabaseConnString, AppName)));
            sr.Register<IPaymentSystemSettingsRepository>(new PaymentSystemSettingsRepository(new PostgresConnection(settingModel.DatabaseConnString, AppName)));
        }

        public static ILogger BindSeqLogger(this IServiceRegistrator sr, SettingModel settingModel)
        {
            var logger = new MyLogger.MyLogger(AppName, settingModel.SeqUrl);

            sr.Register<ILogger>(logger);

            return logger;
        }

        public static void BindQueues(this IServiceRegistrator sr)
        {
            sr.Register(new BaseQueue<PaymentInvoiceEntity>("PaymentInvoiceQueue"));
            sr.Register(new BaseQueue<DepositCallbackEntity>("CallbackQueue"));
        }

        public static MyServiceBusTcpClient BindServiceBus(this IServiceRegistrator sr)
        {
            var tcpServiceBus = new MyServiceBusTcpClient(
                () => SettingModel.ServiceBusWriter, AppName);

            var depositCreatePublisher = new DepositCreateMyServiceBusPublisher(tcpServiceBus);
            var depositUpdateStatusPublisher = new DepositStatusUpdateMyServiceBusPublisher(tcpServiceBus);

            sr.Register<IPublisher<DepositCreateServiceBusContract>>(depositCreatePublisher);
            sr.Register<IPublisher<DepositStatusUpdateServiceBusContract>>(depositUpdateStatusPublisher);
            return tcpServiceBus;
        }

        public static void BindServices(this IServiceRegistrator sr)
        {
            sr.Register<IPaymentSystemSettingsService>(new PaymentSystemSettingsService());
            sr.Register<ILocalCache<IPaymentSystemSettingsService, string, IReadOnlyList<PaymentSystemSettingsEntity>>>(new LocalCache<IPaymentSystemSettingsService, string, IReadOnlyList<PaymentSystemSettingsEntity>>());
        }
    }
}