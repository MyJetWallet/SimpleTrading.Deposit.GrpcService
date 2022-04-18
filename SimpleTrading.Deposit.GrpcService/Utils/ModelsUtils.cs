using System;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SimpleTrading.Deposit.GrpcService.Utils
{
    public static class ModelsUtils
    {
        public const string St = "ST";
        public const string Mt = "MT";
        
        public const string LiveMt = "mtl";
        public const string DemoMt = "mtd";
        public const string LiveSt = "stl";
        public const string DemoSt = "std";
        
        public static bool IsLiveSt(this string accountId)
        {
            return accountId.Contains(LiveSt, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsLiveMt(this string accountId)
        {
            return accountId.Contains(LiveMt, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsSt(this string accountType)
        {
            return accountType == St;
        }
        
        public static bool IsMt(this string accountType)
        {
            return accountType == Mt;
        }

        public static bool IsLive(this string accountId)
        {
            return accountId.Contains(LiveMt, StringComparison.CurrentCultureIgnoreCase) || 
                   accountId.Contains(LiveSt, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsMonfex(this string brandName)
        {
            return brandName.ToLower().Contains("monfex");
        }
        
        public static bool IsHp(this string brandName)
        {
            return brandName.ToLower().Contains("handelpro");
        }
        
        public static string PrettyJson(this string unPrettyJson)
        {
            var options = new JsonSerializerOptions{
                WriteIndented = true
            };

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(unPrettyJson);

            return JsonSerializer.Serialize(jsonElement, options);
        }
    }
}