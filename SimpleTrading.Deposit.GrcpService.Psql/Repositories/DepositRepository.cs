using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MyPostgreSQL;
using Npgsql;
using SimpleTrading.Deposit.GrcpService.Psql.Models;
using SimpleTrading.Payments.Abstractions;

namespace SimpleTrading.Deposit.GrcpService.Psql.Repositories
{
    public class DepositRepository
    {
        private readonly string _dbConnectionString;
        private readonly string _appName;

        public DepositRepository(string connectionString, string appName)
        {
            _dbConnectionString = connectionString;
            _appName = appName;
        }

        public async Task Add(DepositModel item)
        {
            var db = new PostgresConnection(_dbConnectionString, _appName);
            var sql = $"INSERT INTO deposits (id, paymentsystem, psaggregator, srccurrency, srcamount, amount," +
                          " traderid, accountid, datetime, accepted, comment, status, pstransactionid, currency, extraprofit," +
                          $" platformdatetime, {nameof(DepositModel.Type)}) VALUES(@TransactionId,@PaymentSystem,@PaymentProvider,@PsCurrency, @PsAmount, " +
                          " @Amount, @TraderId, @AccountId, @DateTime, @Accepted, @Comment, @Status, @PsTransactionId, @Currency, " +
                          $"0, @PlatformDateTime, @{nameof(DepositModel.Type)})";

            await db.ExecAsync(sql, item);
        }

        public async Task<DepositModel> FindById(string id)
        {
            var db = new PostgresConnection(_dbConnectionString, _appName);
            const string sql = "SELECT * FROM deposits WHERE id = @Id";
            return await db.GetFirstRecordOrNullAsync<DepositModel>(sql, new {Id = id});
        }

        public async Task<IEnumerable<DepositModel>> FindByPsId(string id)
        {
            var db = new PostgresConnection(_dbConnectionString, _appName);

            const string sql = "SELECT * FROM deposits WHERE pstransactionid = @Id";
            return await db.GetRecordsAsync<DepositModel>(sql, new {Id = id});
        }

        public async Task<DepositModel> FindPendingById(string id)
        {
            var db = new PostgresConnection(_dbConnectionString, _appName);
            const string sql = "SELECT * FROM deposits WHERE id = @Id and status = @Status";
            return await db.GetFirstRecordOrNullAsync<DepositModel>(sql, new {Id = id, Status = ((int) PaymentInvoiceStatusEnum.Registered).ToString()});
        }

        public async Task<IEnumerable<DepositModel>> FindPendingByPsId(string id)
        {
            var db = new PostgresConnection(_dbConnectionString, _appName);

            const string sql = "SELECT * FROM deposits WHERE pstransactionid = @Id and status = @Status";
            return await db.GetRecordsAsync<DepositModel>(sql, new {Id = id, Status = ((int) PaymentInvoiceStatusEnum.Registered).ToString()});
        }

        public async Task MakeSuccess(string id, string psTransactionId)
        {
            var db = new PostgresConnection(_dbConnectionString, _appName);
            const string sql =
                @"UPDATE deposits SET status = @Status, pstransactionid = @PsTransactionId, accepted = @AcceptedDate where id = @Id";
            await db.ExecAsync(sql, new
            {
                Status = PaymentInvoiceStatusEnum.Approved,
                PsTransactionId = psTransactionId,
                Id = id,
                AcceptedDate = DateTime.UtcNow
            });
        }

        public async Task MakeFailed(string id, string psTransactionId)
        {
            var db = new PostgresConnection(_dbConnectionString, _appName);
            const string sql =
                "UPDATE deposits SET status = @Status, pstransactionid = @PsTransactionId where id = @Id";
            await db.ExecAsync(sql, new 
            {
                Status = PaymentInvoiceStatusEnum.Failed,
                PsTransactionId = psTransactionId,
                Id = id
            });
        }
    }
}