using SimpleTrading.Deposit.Postgresql.Models;

namespace SimpleTrading.Deposit.GrcpService.Psql.Accounts
{
    public class AccountModel
    {
        public string TraderId { get; set; }
        public string AccountId { get; set; }
        public string BrandName { get; set; }
    }
}