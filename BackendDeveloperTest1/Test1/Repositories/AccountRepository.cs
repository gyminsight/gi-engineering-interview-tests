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
            try
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
            catch 
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid gUid, DapperDbContext dbContext)
        {
            try
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
            catch
            {
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Guid gUid, Account entity, DapperDbContext dbContext)
        {
            if (entity == null) return false;

            try
            {
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
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Account>> GetAllAsync(DapperDbContext dbContext)
        {
            try
            {
                IEnumerable<Account> accounts;

                const string sql = @"SELECT   a.UID,
                                          l.Guid AS LocationGuid,
                                          a.LocationUid,
                                          a.Guid,
                                          a.CreatedUtc,
                                          a.UpdatedUtc,
                                          a.Status,
                                          a.EndDateUtc,
                                          a.AccountType,
                                          a.PaymentAmount,
                                          a.PendCancel,
                                          a.PendCancelDateUtc,
                                          a.PeriodStartUtc,
                                          a.PeriodEndUtc,
                                          a.NextBillingUtc
                                   FROM   account a
                                   INNER JOIN Location l on a.LocationUid = l.UID;";

                var builder = new SqlBuilder();

                var template = builder.AddTemplate(sql);

                accounts = await dbContext.Session.QueryAsync<Account>(template.RawSql, template.Parameters, dbContext.Transaction);

                return accounts;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Account> GetByIdAsync(Guid gUid, DapperDbContext dbContext)
        {
            try
            {
                Account account;

                const string sql = @"SELECT   a.UID,
                                          l.Guid AS LocationGuid,
                                          a.LocationUid,
                                          a.Guid,
                                          a.CreatedUtc,
                                          a.UpdatedUtc,
                                          a.Status,
                                          a.EndDateUtc,
                                          a.AccountType,
                                          a.PaymentAmount,
                                          a.PendCancel,
                                          a.PendCancelDateUtc,
                                          a.PeriodStartUtc,
                                          a.PeriodEndUtc,
                                          a.NextBillingUtc
                                   FROM   account a
                                   INNER JOIN Location l on a.LocationUid = l.UID
                                  WHERE a.Guid = @Guid";

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
            catch
            {
                throw;
            }
        }
    }
}
