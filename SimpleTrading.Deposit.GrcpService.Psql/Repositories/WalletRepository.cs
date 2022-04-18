using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPostgreSQL;
using SimpleTrading.Deposit.GrcpService.Psql.Models;

namespace SimpleTrading.Deposit.GrcpService.Psql.Repositories
{
    public class WalletRepository
    {
        private readonly IPostgresConnection _postgresConnection;

        public WalletRepository(IPostgresConnection postgresConnection)
        {
            _postgresConnection = postgresConnection ?? throw new ArgumentNullException(nameof(postgresConnection));
        }

        public async Task<IEnumerable<WalletModel>> GetByTraderId(string traderId)
        {
            const string sql = "SELECT * FROM wallets where traderid = @TraderId";
            return await _postgresConnection.GetRecordsAsync<WalletModel>(sql, new {TraderId = traderId});
        }
        
        public async Task<WalletModel> GetStTraderAccount(string traderId)
        {
            const string sql = "SELECT * FROM wallets where traderid = @TraderId and accountid LIKE '%stl%'";
            return await _postgresConnection.GetFirstRecordOrNullAsync<WalletModel>(sql, new
            {
                TraderId = traderId
            });
        }

        public async Task<WalletModel> GetMtTraderAccount(string traderId)
        {
            const string sql = "SELECT * FROM wallets where traderid = @TraderId and accountid LIKE '%mtl%'";
            return await _postgresConnection.GetFirstRecordOrNullAsync<WalletModel>(sql, new
            {
                TraderId = traderId,
            });
        }
    }
}