using MyPostgreSQL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTrading.Deposit.GrcpService.Psql.Accounts
{
    public class AccountRepository: IAccountRepository
    {
        private readonly IPostgresConnection _postgresConnection;

        public AccountRepository(IPostgresConnection postgresConnection)
        {
            _postgresConnection = postgresConnection ?? throw new ArgumentNullException(nameof(postgresConnection));
        }

        public async Task<string> GetBrandAsync(string traderId)
        {
            const string sql = @"
select ma.traderid, ma.accountid, pdv.brandid from mt5_accounts ma
 right join personaldata pdv
 on pdv.id = ma.traderid
  where pdv.id = @TraderId";

            var result =  await _postgresConnection.GetFirstRecordOrNullAsync<AccountModel>(sql, new
            {
                TraderId = traderId
            });

            return result?.BrandName;
        }
    }
}
