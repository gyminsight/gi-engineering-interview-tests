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
        private readonly IRepository<Member> _repositoryMember;
        private readonly ISessionFactory _sessionFactory;

        public AccountService(IRepository<Account> accountRepository, IRepository<Member> memberRepository, ISessionFactory session)
        {
            _repository = accountRepository;
            _repositoryMember = memberRepository;
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

        public async Task<IEnumerable<MemberReadDto>> GetAllMembersByAccountAsync(Guid accountGuid, CancellationToken cancellationToken)
        {
            var currentAccount = await GetAccountByIdAsync(accountGuid, cancellationToken);

            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken);
            

            if (currentAccount != null)
            {
                var members = await _repositoryMember.GetAllByIdAsync(currentAccount.Uid, dbContext);
                dbContext.Commit();
                return members.ToList().Select(e => new MemberReadDto
                {
                    Uid = e.Uid,
                    Guid = e.Guid,
                    AccountUid = e.AccountUid,
                    LocationUid = e.LocationUid,
                    CreatedUtc = e.CreatedUtc,
                    UpdatedUtc = e.UpdatedUtc,
                    Primary = e.Primary,
                    JoinedDateUtc = e.JoinedDateUtc,
                    CancelDateUtc = e.CancelDateUtc,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Address = e.Address,
                    City = e.City,
                    Locale = e.Locale,
                    PostalCode = e.PostalCode,
                    Cancelled = e.Cancelled
                });
            }
            return Enumerable.Empty<MemberReadDto>(); 
        }
    }
}
