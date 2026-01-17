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

        public MemberService(ISessionFactory session, IReadOnlyRepository<Location> readOnlyRepository, IRepository<Account> accountRepository, IMemberRepository repositoryMember)
        {
            _sessionFactory = session;
            _readOnlyRepository = readOnlyRepository;
            _accountRepository = accountRepository;
            _repositoryMember = repositoryMember;
        }

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
            catch (Exception ex)
            {
                dbContext.Rollback();
                throw ex;
            }
        }

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
        public async Task<IEnumerable<MemberReadDto>> GetAllMembersByAccountAsync(Guid accountGuid, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken);

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
    }
}
