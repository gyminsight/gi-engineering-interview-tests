using System;
using Test1.Contracts;
using Test1.DTOs;
using Test1.Exceptions;
using Test1.Interfaces;
using Test1.Models;

namespace Test1.Services
{
    public class MemberService : IMemberService
    {
        private readonly ISessionFactory _sessionFactory;
        private readonly IReadOnlyRepository<Location> _readOnlyRepository;
        private readonly IRepository<Account> _accountRepository;
        private readonly IMemberRepository _repositoryMember;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="session">Factory for creating database session contexts.</param>
        /// <param name="readOnlyRepository">Read-only repository for Location entity queries.</param>
        /// <param name="accountRepository">Repository for Account entity operations.</param>
        /// <param name="repositoryMember">Repository for Member entity operations.</param>
        public MemberService(ISessionFactory session, IReadOnlyRepository<Location> readOnlyRepository, IRepository<Account> accountRepository, IMemberRepository repositoryMember)
        {
            _sessionFactory = session;
            _readOnlyRepository = readOnlyRepository;
            _accountRepository = accountRepository;
            _repositoryMember = repositoryMember;
        }

        /// <summary>
        /// Creates a new member with the provided member data.
        /// </summary>
        /// <param name="member">The member data transfer object containing member information.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>True if member was successfully created, false otherwise.</returns>
        /// <exception cref="PrimaryMemberException">Thrown when attempting to create a primary member when one already exists for the account.</exception>
        public async Task<bool> CreateMemberAsync(MemberCreateDto member, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                bool primaryAlreadyExist = await _repositoryMember.ExistingPrimaryMemberByAccountValidation(member.AccountGuid, dbContext);

                if (primaryAlreadyExist && member.Primary)
                {
                    throw new PrimaryMemberException("There is an existing Primary member for the selected account.");
                } 

                var location = await _readOnlyRepository.GetByIdAsync(member.LocationGuid, dbContext);

                var account = await _accountRepository.GetByIdAsync(member.AccountGuid, dbContext);

                var entity = new Member
                {
                    AccountUid = account.Uid,
                    LocationUid = location.UID,
                    Primary = member.Primary,
                    JoinedDateUtc = member.JoinedDateUtc,
                    FirstName = member.FirstName,
                    LastName = member.LastName,
                    Address = member.Address,
                    City = member.City,
                    Locale = member.Locale,
                    PostalCode = member.PostalCode,
                    CancelDateUtc = member.CancelDateUtc,
                    Cancelled = member.Cancelled,
                    Guid = Guid.NewGuid(),
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow
                };
                var added = await _repositoryMember.AddAsync(entity, dbContext);
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
        /// Deletes a member by its unique identifier. Automatically promotes another member to primary if the deleted member was primary.
        /// </summary>
        /// <param name="Guid">The unique identifier of the member to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>True if member was successfully deleted, false otherwise.</returns>
        /// <exception cref="LastAccountMemberException">Thrown when attempting to delete the last remaining member of an account.</exception>
        public async Task<bool> DeleteMemberAsync(Guid Guid, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                var currentMember = await _repositoryMember.GetByIdAsync(Guid, dbContext);

                bool canDelete = await _repositoryMember.LastAccountMemberValidation(currentMember.AccountGuid, dbContext);

                if (!canDelete)
                {
                    throw new LastAccountMemberException("Cannot delete the last member of the account.");
                }

                if (currentMember != null && currentMember.Primary) 
                {
                    var members = await _repositoryMember.GetAllMembersByAccountAsync(currentMember.AccountGuid, dbContext);
                    var anotherMember = members.Where(m => m.Guid != Guid);
                    if (anotherMember.Any())
                    {
                        var newPrimaryMember = anotherMember.First();
                        newPrimaryMember.Primary = true;
                        await _repositoryMember.UpdateAsync(newPrimaryMember.Guid,newPrimaryMember, dbContext);
                    }
                }

                var deleted = await _repositoryMember.DeleteAsync(Guid, dbContext);
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
        /// Retrieves all members.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Enumerable collection of MemberReadDto objects containing all members.</returns>
        public async  Task<IEnumerable<MemberReadDto>> GetAllMembersAsync(CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken);

            try
            {
                var entities = await _repositoryMember.GetAllAsync(dbContext);
                dbContext.Commit();
                return entities.Select(e => new MemberReadDto
                {
                Guid = e.Guid,
                LocationGuid = e.LocationGuid,
                AccountGuid = e.AccountGuid,
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
                PostalCode = e.PostalCode
                });
            }
            catch
            {
                dbContext.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific member by its unique identifier.
        /// </summary>
        /// <param name="gUid">The unique identifier of the member.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>MemberReadDto containing member details, or null if member not found.</returns>
        public async Task<MemberReadDto> GetMemberByIdAsync(Guid gUid, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                var entity = await _repositoryMember.GetByIdAsync(gUid, dbContext);
                dbContext.Commit();
                return entity == null ? null : new MemberReadDto
                {
                Guid = entity.Guid,
                LocationGuid = entity.LocationGuid,
                AccountGuid = entity.AccountGuid,
                CreatedUtc = entity.CreatedUtc,
                UpdatedUtc = entity.UpdatedUtc,
                Primary = entity.Primary,
                JoinedDateUtc = entity.JoinedDateUtc,
                CancelDateUtc = entity.CancelDateUtc,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Address = entity.Address,
                City = entity.City,
                Locale = entity.Locale,
                PostalCode = entity.PostalCode
                };
            }
            catch
            {
                dbContext.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Retrieves all members belonging to a specific account.
        /// </summary>
        /// <param name="accountGuid">The unique identifier of the account.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>Enumerable collection of MemberReadDto objects for the specified account.</returns>
        public async Task<IEnumerable<MemberReadDto>> GetAllMembersByAccountAsync(Guid accountGuid, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                var members = await _repositoryMember.GetAllMembersByAccountAsync(accountGuid, dbContext);

                dbContext.Commit();
                return members.Select(e => new MemberReadDto
                {
                    Guid = e.Guid,
                    AccountGuid = e.AccountGuid,
                    LocationGuid = e.LocationGuid,
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
            catch
            {
                dbContext.Rollback();
                throw;
            }
        }
    }
}
