using SimpleTrading.SettingsReader;

namespace SimpleTrading.Deposit.GrpcService
{
    [YamlAttributesOnly]
    public class SettingModel
    {
        [YamlProperty("DepositManager.ServiceBusWriter")]
        public static string ServiceBusWriter { get; internal set; }
        [YamlProperty("DepositManager.Db.AzureStorageConnString")]
        public string AzureStorageConnString { get; set; }

        [YamlProperty("DepositManager.Db.DatabaseConnString")]
        public string DatabaseConnString { get; set; }

        [YamlProperty("DepositManager.Db.AzureCreateInvoiceRequestLogTable")]
        public string AzureCreateInvoiceRequestLogTable { get; set; }

        [YamlProperty("DepositManager.Db.AzureCreateInvoiceCallbackLogTable")]
        public string AzureCreateInvoiceCallbackLogTable { get; set; }

        [YamlProperty("DepositManager.Lp.Exactly.BearerToken")]
        public string ExactlyBearerToken { get; set; }

        [YamlProperty("DepositManager.Lp.Exactly.ApiHost")]
        public string ExactlyApiHost { get; set; }

        [YamlProperty("DepositManager.Lp.Texcent.BaseUrl")]
        public string TexcentBaseUrl { get; set; }

        [YamlProperty("DepositManager.SuccessUrl")]
        public string SuccessPage { get; set; }

        [YamlProperty("DepositManager.FailPage")]
        public string FailPage { get; set; }

        [YamlProperty("DepositManager.Callback")]
        public string CallbackUrl { get; set; }

        [YamlProperty("DepositManager.EngineLiveGrpcServerUrl")]
        public string EngineLiveGrpcServerUrl { get; set; }
        
        [YamlProperty("DepositManager.ConvertGrpcServerUrl")]
        public string ConvertGrpcServerUrl { get; set; }
        
        [YamlProperty("DepositManager.AuthGrpcServiceUrl")]
        public string AuthGrpcServiceUrl { get; set; }

        [YamlProperty("DepositManager.SeqUrl")]
        public string SeqUrl { get; set; }
        
        [YamlProperty("DepositManager.MtBridgeLive")]
        public string MtBridgeLive { get; set; }       
        
        [YamlProperty("DepositManager.AuditLogGrpcService")]
        public string AuditLogGrpcService { get; set; }

        [YamlProperty("DepositManager.PaymentSystemCacheExpirationMin")]
        public int PaymentSystemCacheExpirationMin { get; set; }
    }
}