using Dapper;
using Microsoft.AspNetCore.Http;
using System.Threading;
using Test1.Contracts;
using Test1.Core;
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
        private readonly IReadOnlyRepository<Location> _readOnlyRepository;
        private readonly IMemberRepository _memberRepository;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="accountRepository">Repository for Account entity operations.</param>
        /// <param name="session">Factory for creating database session contexts.</param>
        /// <param name="readOnlyRepository">Read-only repository for Location entity queries.</param>
        /// <param name="memberRepository">Repository for Member entity operations.</param>
        public AccountService(IRepository<Account> accountRepository, ISessionFactory session, IReadOnlyRepository<Location> readOnlyRepository, IMemberRepository memberRepository)
        {
            _repository = accountRepository;
            _sessionFactory = session;
            _readOnlyRepository = readOnlyRepository;
            _memberRepository = memberRepository;
        }

        /// <summary>
        /// Creates a new account with the provided account data.
        /// </summary>
        /// <param name="accountCreateDto">The account data transfer object containing account information.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>True if account was successfully created, false otherwise.</returns>
        public async Task<bool> CreateAccountAsync(AccountCreateDto accountCreateDto, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                var location = await _readOnlyRepository.GetByIdAsync(accountCreateDto.LocationGuid, dbContext);

                var entity = new Account { 
                    LocationUid = location.UID,
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

        /// <summary>
        /// Deletes an account and its associated data by unique identifier.
        /// </summary>
        /// <param name="gUid">The unique identifier of the account to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>True if account was successfully deleted, false otherwise.</returns>
        public async Task<bool> DeleteAccountAsync(Guid gUid, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                var deleted = await _repository.DeleteAsync(gUid, dbContext);
                dbContext.Commit();
                return deleted;
            }
            catch
            {
                dbContext.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Deletes all non-primary members associated with an account.
        /// </summary>
        /// <param name="id">The unique identifier of the account.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>True if non-primary members were successfully deleted, false otherwise.</returns>
        public async Task<bool> DeleteNonPrimaryMembersAsync(Guid id, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                bool deleted = await _memberRepository.DeleteNonPrimaryMembersAsyncByAccount(id, dbContext);
                dbContext.Commit();
                return deleted;
            }
            catch (Exception)
            {
                dbContext.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific account by its unique identifier.
        /// </summary>
        /// <param name="gUid">The unique identifier of the account.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>AccountReadDto containing account details, or null if account not found.</returns>
        public async Task<AccountReadDto> GetAccountByIdAsync(Guid gUid, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                var entity = await _repository.GetByIdAsync(gUid, dbContext);
                dbContext.Commit();
                return entity == null ? null : new AccountReadDto { 
                    LocationGuid = entity.LocationGuid,
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

        /// <summary>
        /// Retrieves all accounts.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Enumerable collection of AccountReadDto objects containing all accounts.</returns>
        public async Task<IEnumerable<AccountReadDto>> GetAllAccountsAsync(CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken);

            try
            {
                var entities = await _repository.GetAllAsync(dbContext);
                dbContext.Commit();
                return entities.ToList().Select(e => new AccountReadDto 
                {
                    Guid = e.Guid,
                    LocationGuid = e.LocationGuid,
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
            }
            catch 
            {
                dbContext.Rollback();
                throw ;
            }
        }

        /// <summary>
        /// Updates an existing account with the provided account data.
        /// </summary>
        /// <param name="gUid">The unique identifier of the account to update.</param>
        /// <param name="accountUpdateDto">The account data transfer object containing updated account information.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>True if account was successfully updated, false otherwise.</returns>
        public async Task<bool> UpdateAccountAsync(Guid gUid, AccountUpdateDto accountUpdateDto, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                var location = await _readOnlyRepository.GetByIdAsync(accountUpdateDto.LocationGuid, dbContext);

                var entity = new Account 
                { 
                    LocationUid = location.UID,
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
