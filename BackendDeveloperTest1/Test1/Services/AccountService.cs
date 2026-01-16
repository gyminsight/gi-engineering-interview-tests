using Microsoft.AspNetCore.Http;
using System.Threading;
using Test1.Contracts;
using Test1.DTOs;
using Test1.Interfaces;
using Test1.Models;
using static Dapper.SqlMapper;

namespace Test1.Services
{
    public class AccountService : IAccountService
    {
        private readonly IRepository<Account> _repository;
        private readonly ISessionFactory _sessionFactory;

        public AccountService(IRepository<Account> accountRepository, ISessionFactory session)
        {
            _repository = accountRepository;
            _sessionFactory = session;
        }

        public async Task<bool> CreateAccountAsync(AccountCreateDto accountCreateDto, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                var entity = new Account { 
                    LocationUid = accountCreateDto.LocationUid,
                    Guid = Guid.NewGuid(),
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow,
                    Status = accountCreateDto.Status,
                    AccountType = accountCreateDto.AccountType,
                    PeriodStartUtc = accountCreateDto.PeriodStartUtc,
                    PeriodEndUtc = accountCreateDto.PeriodEndUtc,
                    NextBillingUtc = accountCreateDto.NextBillingUtc,
                    PaymentAmount = accountCreateDto.PaymentAmount,
                    PendCancel = accountCreateDto.PendCancel,
                    PendCancelDateUtc = accountCreateDto.PendCancelDateUtc,
                    EndDateUtc = accountCreateDto.EndDateUtc
                };
                var added = await _repository.AddAsync(entity, dbContext);
                dbContext.Commit();
                return added;
            }
            catch
            {
                dbContext.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAccountAsync(Guid gUid, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                var entity = new Account { Guid = gUid };
                var deleted = await _repository.DeleteAsync(entity, dbContext);
                dbContext.Commit();
                return deleted;
            }
            catch
            {
                dbContext.Rollback();
                throw;
            }
        }

        public async Task<AccountReadDto> GetAccountByIdAsync(Guid gUid, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                var entity = await _repository.GetByIdAsync(gUid, dbContext);
                dbContext.Commit();
                return entity == null ? null : new AccountReadDto { 
                    Uid = entity.Uid,
                    LocationUid = entity.LocationUid,
                    Guid = entity.Guid,
                    CreatedUtc = entity.CreatedUtc,
                    UpdatedUtc = entity.UpdatedUtc,
                    Status = entity.Status,
                    EndDateUtc = entity.EndDateUtc,
                    AccountType = entity.AccountType,
                    PaymentAmount = entity.PaymentAmount,
                    PendCancel = entity.PendCancel,
                    PendCancelDateUtc = entity.PendCancelDateUtc,
                    PeriodStartUtc = entity.PeriodStartUtc,
                    PeriodEndUtc = entity.PeriodEndUtc,
                    NextBillingUtc = entity.NextBillingUtc
                };
            }
            catch
            {
                dbContext.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<AccountReadDto>> GetAllAccountsAsync(CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken);
            
            //try
            //{
                var entities = await _repository.GetAllAsync(dbContext);
                dbContext.Commit();
                return entities.ToList().Select(e => new AccountReadDto 
                {
                    Uid = e.Uid,
                    LocationUid = e.LocationUid,
                    Guid = e.Guid,
                    CreatedUtc = e.CreatedUtc,
                    UpdatedUtc = e.UpdatedUtc,
                    Status = e.Status,
                    EndDateUtc = e.EndDateUtc,
                    AccountType = e.AccountType,
                    PaymentAmount = e.PaymentAmount,
                    PendCancel = e.PendCancel,
                    PendCancelDateUtc = e.PendCancelDateUtc,
                    PeriodStartUtc = e.PeriodStartUtc,
                    PeriodEndUtc = e.PeriodEndUtc,
                    NextBillingUtc = e.NextBillingUtc
                });
            //}
            //catch(Exception ex)
            //{
            //    //dbContext.Rollback();
            //    throw ex;
            //}
        }

        public async Task<bool> UpdateAccountAsync(Guid gUid, AccountUpdateDto accountUpdateDto, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                var entity = new Account 
                { 
                    LocationUid = accountUpdateDto.LocationUid,
                    UpdatedUtc = DateTime.UtcNow,
                    Status = accountUpdateDto.Status,
                    EndDateUtc = accountUpdateDto.EndDateUtc,
                    AccountType = accountUpdateDto.AccountType,
                    PaymentAmount = accountUpdateDto.PaymentAmount,
                    PendCancel = accountUpdateDto.PendCancel,
                    PendCancelDateUtc = accountUpdateDto.PendCancelDateUtc,
                    PeriodStartUtc = accountUpdateDto.PeriodStartUtc,
                    PeriodEndUtc = accountUpdateDto.PeriodEndUtc,
                    NextBillingUtc = accountUpdateDto.NextBillingUtc
                };
                var updated = await _repository.UpdateAsync(gUid, entity, dbContext);
                dbContext.Commit();

                return updated;
            }
            catch
            {
                dbContext.Rollback();
                throw;
            }
        }
    }
}
