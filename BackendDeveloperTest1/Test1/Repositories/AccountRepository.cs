using Dapper;
using System.Security.Principal;
using Test1.Contracts;
using Test1.Core;
using Test1.DTOs;
using Test1.Interfaces;
using Test1.Models;

namespace Test1.Repositories
{
    public class AccountRepository : IRepository<Account>
    {
        public async Task<bool> AddAsync(Account entity, DapperDbContext dbContext)
        {
            const string sql = @"
                                INSERT INTO account (
                                      
                                      LocationUid,
                                      Guid,
                                      CreatedUtc,
                                      UpdatedUtc,
                                      Status,
                                      EndDateUtc,
                                      AccountType,
                                      PaymentAmount,
                                      PendCancel,
                                      PendCancelDateUtc,
                                      PeriodStartUtc,
                                      PeriodEndUtc,
                                      NextBillingUtc
                                ) VALUES (
                                    @LocationUid,
                                    @Guid,
                                    @CreatedUtc,
                                    @UpdatedUtc,
                                    @Status,
                                    @EndDateUtc,
                                    @AccountType,
                                    @PaymentAmount,
                                    @PendCancel,
                                    @PendCancelDateUtc,
                                    @PeriodStartUtc,
                                    @PeriodEndUtc,
                                    @NextBillingUtc
                                );";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql, new
            {
                entity.LocationUid,
                entity.Guid,
                entity.CreatedUtc,
                entity.UpdatedUtc,
                entity.Status,
                entity.AccountType,
                entity.PeriodStartUtc,
                entity.PeriodEndUtc,
                entity.NextBillingUtc,
                entity.PaymentAmount,
                entity.PendCancel,
                entity.PendCancelDateUtc,
                entity.EndDateUtc

            });

            var affectedRows = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(Guid gUid, DapperDbContext dbContext)
        {

            const string sql = "DELETE FROM account WHERE Guid = @gUid;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql, new
            {
                gUid = gUid
            });

            var affectedRows = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            return affectedRows > 0;
        }

        public async Task<bool> UpdateAsync(Guid gUid, Account entity, DapperDbContext dbContext)
        {
            if (entity == null) return false;

            const string sql = @"
                           UPDATE account
                              SET LocationUid = @LocationUid,
                                  UpdatedUtc = @UpdatedUtc,
                                  Status = @Status,
                                  EndDateUtc = @EndDateUtc,
                                  AccountType = @AccountType,
                                  PaymentAmount = @PaymentAmount,
                                  PendCancel = @PendCancel,
                                  PendCancelDateUtc = @PendCancelDateUtc,
                                  PeriodStartUtc = @PeriodStartUtc,
                                  PeriodEndUtc = @PeriodEndUtc,
                                  NextBillingUtc = @NextBillingUtc
                            WHERE Guid = @gUid;";

            var builder = new SqlBuilder();
            var template = builder.AddTemplate(sql, new
            {
                entity.LocationUid,
                entity.UpdatedUtc,
                entity.Status,
                entity.EndDateUtc,
                entity.AccountType,
                entity.PaymentAmount,
                entity.PendCancel,
                entity.PendCancelDateUtc,
                entity.PeriodStartUtc,
                entity.PeriodEndUtc,
                entity.NextBillingUtc,
                gUid
            });

            var affectedRows = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
               .ConfigureAwait(false);

            return affectedRows > 0;
        }

        public async Task<IEnumerable<Account>> GetAllAsync(DapperDbContext dbContext)
        {
            IEnumerable<Account> accounts;

            const string sql = @"SELECT   UID,
                                          LocationUid,
                                          Guid,
                                          CreatedUtc,
                                          UpdatedUtc,
                                          Status,
                                          EndDateUtc,
                                          AccountType,
                                          PaymentAmount,
                                          PendCancel,
                                          PendCancelDateUtc,
                                          PeriodStartUtc,
                                          PeriodEndUtc,
                                          NextBillingUtc
                                   FROM   account;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            accounts = await dbContext.Session.QueryAsync<Account>(template.RawSql, template.Parameters, dbContext.Transaction);

            return accounts;
        }

        public async Task<Account> GetByIdAsync(Guid gUid, DapperDbContext dbContext)
        {
            Account account;

            const string sql = @"SELECT   UID,
                                          LocationUid,
                                          Guid,
                                          CreatedUtc,
                                          UpdatedUtc,
                                          Status,
                                          EndDateUtc,
                                          AccountType,
                                          PaymentAmount,
                                          PendCancel,
                                          PendCancelDateUtc,
                                          PeriodStartUtc,
                                          PeriodEndUtc,
                                          NextBillingUtc
                                   FROM account
                                  WHERE Guid = @Guid";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            builder.Where("Guid = @gUid", new
            {
                Guid = gUid
            });

            account = await dbContext.Session.QueryFirstOrDefaultAsync<Account>(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            return (account);
        }

    }
}
