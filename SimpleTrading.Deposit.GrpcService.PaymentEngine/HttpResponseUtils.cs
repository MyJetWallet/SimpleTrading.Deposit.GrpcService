using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleTrading.Deposit.GrpcService.PaymentEngine
{
    public static class HttpResponseUtils
    {
        public static async Task<T> ProcessPaymentProviderResponse<T>(this HttpResponseMessage message)
        {
            if (!message.IsSuccessStatusCode)
                throw new HttpRequestException(message.Content.ReadAsStringAsync().Result);
            
            var result = await message.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(result);
        } 
    }
}