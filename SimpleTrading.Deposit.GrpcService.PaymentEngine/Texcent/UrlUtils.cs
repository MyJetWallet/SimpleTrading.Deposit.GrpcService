namespace SimpleTrading.Deposit.GrpcService.PaymentEngine.Texcent
{
    public static class UrlUtils
    {
        public static string AppendGetParam(this string str, string key, string value)
        {
            return str + '&' + key + '=' + System.Net.WebUtility.UrlEncode(value);
        }
    }
}