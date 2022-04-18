using System;
using System.Threading.Tasks;
using MyPostgreSQL;
using SimpleTrading.Deposit.GrcpService.Psql.Accounts.Models;

namespace SimpleTrading.Deposit.GrcpService.Psql.Accounts
{
    public class Mt5AccountsRepository: IMt5AccountsRepository
    {
        private readonly IPostgresConnection _postgresConnection;


        public Mt5AccountsRepository(IPostgresConnection postgresConnection)
        {
            _postgresConnection = postgresConnection ?? throw new ArgumentNullException(nameof(postgresConnection));
        }

        public async Task<Mt5AccountModel> GetTraderAccount(string accountId, string traderId)
        {
            const string sql = @"select registered, traderid, accountid, balance, equity, islive, login, leverage , ""group"", accounttype, margin, marginfree, currency " +
                "from mt5_accounts where traderid = @TraderId and accountid = @AccountId";

            return await _postgresConnection.GetFirstRecordOrNullAsync<Mt5AccountModel>(sql, new
            {
                TraderId = traderId,
                AccountId = accountId
            });
        }

        public async Task<long> GetTraderMt5Login(string accountId, string traderId)
        {
            const string sql = @"select login " +
                               "from mt5_accounts where traderid = @TraderId and accountid = @AccountId";

            return await _postgresConnection.GetFirstRecordOrNullAsync<long>(sql, new
            {
                TraderId = traderId,
                AccountId = accountId
            });
        }

        public async Task<Mt5AccountModel> GetTraderAccount(string accountId)
        {
            const string sql = @"select registered, traderid, accountid, balance, equity, islive, login, leverage , ""group"", accounttype, margin, marginfree, currency " + 
                               "from mt5_accounts where accountid = @AccountId";

            return await _postgresConnection.GetFirstRecordOrNullAsync<Mt5AccountModel>(sql, new
            {
                AccountId = accountId
            });
        }

        public async Task<Mt5AccountModel> UpdateOnCallback(string traderId, string accountId, double margin,
            double marginFree, double profit, double balance, double equity)
        {
            const string sql =
                @"update mt5_accounts set margin = @Margin,
                        marginfree = @MarginFree,
                        profit = @Profit,
                        balance = @Balance,
                        equity = @Equity
                   where traderid = @TraderId and accountid = @AccountId";

            return await _postgresConnection.GetFirstRecordOrNullAsync<Mt5AccountModel>(sql, new
            {
                TraderId = traderId,
                AccountId = accountId,
                Margin = margin,
                MarginFree = marginFree,
                Profit = profit,
                Balance = balance,
                Equity = equity
            });
        }
    }
}