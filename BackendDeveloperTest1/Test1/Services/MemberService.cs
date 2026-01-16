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

        public MemberService(IRepository<Member> accountRepository, ISessionFactory session)
        {
            _repository = accountRepository;
            _sessionFactory = session;
        }

        public async Task<bool> CreateMemberAsync(MemberCreateDto member, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

            try
            {
                var entity = new Member
                {
                    AccountUid = member.AccountUid,
                    LocationUid = member.LocationUid,
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
                Uid = entity.Uid,
                Guid = entity.Guid,
                AccountUid = entity.AccountUid,
                LocationUid = entity.LocationUid,
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
