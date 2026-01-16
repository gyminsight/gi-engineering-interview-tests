using Dapper;
using Test1.Contracts;
using Test1.Core;
using Test1.DTOs;
using Test1.Interfaces;
using Test1.Models;

namespace Test1.Services
{
    public class GetMembersByAccountService : IGetMembersByAccountService
    {
        private readonly ISessionFactory _sessionFactory;

        public GetMembersByAccountService(ISessionFactory session)
        {
            _sessionFactory = session;
        }

        public async Task<IEnumerable<MemberReadDto>> GetAllMembersByAccountAsync(Guid accountGuid, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken);

            var members = await GetAllByAccountGuIdAsync(accountGuid, dbContext);

            dbContext.Commit();
            return members.Select(e => new MemberReadDto
            {
                Guid = e.Guid,
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

        private async Task<IEnumerable<Member>> GetAllByAccountGuIdAsync(Guid accountId, DapperDbContext dbContext)
        {
            IEnumerable<Member> membersByAccount;

            const string sql = @"SELECT m.UID,
                                        m.Guid,
                                        m.AccountUid,
                                        m.LocationUid,
                                        m.CreatedUtc,
                                        m.UpdatedUtc,
                                        m.'Primary',
                                        m.JoinedDateUtc,
                                        m.CancelDateUtc,
                                        m.FirstName,
                                        m.LastName,
                                        m.Address,
                                        m.City,
                                        m.Locale,
                                        m.PostalCode,
                                        m.Cancelled
                                   FROM member m
                                   INNER JOIN account a ON m.AccountUid = a.UID
                                  WHERE a.Guid = @id;";

            var builder = new SqlBuilder();

            builder.Where("AccountUid = @gUid", new
            {
                id = accountId
            });

            var template = builder.AddTemplate(sql);

            membersByAccount = await dbContext.Session.QueryAsync<Member>(template.RawSql, template.Parameters, dbContext.Transaction);

            return membersByAccount;
        }
    }
}
