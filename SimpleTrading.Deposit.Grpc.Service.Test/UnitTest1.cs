using System.Threading.Tasks;
using MyPostgreSQL;
using NUnit.Framework;
using SimpleTrading.Deposit.GrcpService.Psql.Repositories;

namespace SimpleTrading.Deposit.Grpc.Service.Test
{
    public class Tests
    {
        private WalletRepository _repository;
        private string _databaseConnString = "***";
        private string _appName;

        [SetUp]
        public void Setup()
        {
            _repository = new WalletRepository(
                new PostgresConnection(_databaseConnString, _appName));
        }

        [Test]
        public async Task TestMt()
        {
            Assert.Pass();
            var result = await _repository
                .GetMtTraderAccount("7023359af6bb46b284ee461f0b6e2c58");

        }

        [Test]
        public async Task TestSt()
        {
            Assert.Pass();
            var result = await _repository
                .GetStTraderAccount("7023359af6bb46b284ee461f0b6e2c58");
        }
    }
}