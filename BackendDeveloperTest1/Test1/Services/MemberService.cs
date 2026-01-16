using System;
using Test1.Contracts;
using Test1.DTOs;
using Test1.Interfaces;
using Test1.Models;

namespace Test1.Services
{
    public class MemberService : IMemberService
    {
        private readonly IRepository<Member> _repository;
        private readonly ISessionFactory _sessionFactory;
        private readonly IReadOnlyRepository<Location> _readOnlyRepository;
        private readonly IRepository<Account> _accountRepository;

        public MemberService(IRepository<Member> memberRepository, ISessionFactory session, IReadOnlyRepository<Location> readOnlyRepository, IRepository<Account> accountRepository)
        {
            _repository = memberRepository;
            _sessionFactory = session;
            _readOnlyRepository = readOnlyRepository;
            _accountRepository = accountRepository;
        }

        public async Task<bool> CreateMemberAsync(MemberCreateDto member, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
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

        public async Task<bool> DeleteMemberAsync(Guid Guid, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                var deleted = await _repository.DeleteAsync(Guid, dbContext);
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
                var entities = await _repository.GetAllAsync(dbContext);
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
                var entity = await _repository.GetByIdAsync(gUid, dbContext);
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
    }
}
