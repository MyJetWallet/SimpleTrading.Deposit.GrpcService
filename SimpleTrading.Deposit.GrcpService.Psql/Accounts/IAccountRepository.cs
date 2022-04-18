using System.Threading.Tasks;

namespace SimpleTrading.Deposit.GrcpService.Psql.Accounts
{
    public interface IAccountRepository
    {
        Task<string> GetBrandAsync(string traderId);
    }
}