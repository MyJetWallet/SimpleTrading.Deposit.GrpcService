using System;

namespace SimpleTrading.Deposit.GrcpService.Psql.Accounts.Models
{
    public class Mt5AccountModel
    {
        public DateTime Registered { get; set; }
        public string TraderId { get; set; }
        public string AccountId { get; set; }
        public double Balance { get; set; }
        public double Equity { get; set; }
        public bool IsLive { get; set; }
        public long Login { get; set; }
        public int Leverage { get; set; }
        public string Group { get; set; }
        public string AccountType { get; set; }
        public double Margin { get; set; }
        public double MarginFree { get; set; }
        public string Currency { get; set; }
    }
}