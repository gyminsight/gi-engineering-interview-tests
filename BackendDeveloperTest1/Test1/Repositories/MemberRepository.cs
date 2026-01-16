using Dapper;
using System;
using System.Threading;
using Test1.Contracts;
using Test1.Core;
using Test1.Interfaces;
using Test1.Models;

namespace Test1.Repositories
{
    public class MemberRepository : IRepository<Member>
    {
        public async Task<bool> AddAsync(Member entity, DapperDbContext dbContext)
        {
            const string sql = @"
                                INSERT INTO member (
                                    Guid,
                                    AccountUid,
                                    LocationUid,
                                    CreatedUtc,
                                    UpdatedUtc,
                                    'Primary',
                                    JoinedDateUtc,
                                    CancelDateUtc,
                                    FirstName,
                                    LastName,
                                    Address,
                                    City,
                                    Locale,
                                    PostalCode,
                                    Cancelled
                                ) VALUES (
                                    @Guid,
                                    @AccountUid,
                                    @LocationUid,
                                    @CreatedUtc,
                                    @UpdatedUtc,
                                    @Primary,
                                    @JoinedDateUtc,
                                    @CancelDateUtc,
                                    @FirstName,
                                    @LastName,
                                    @Address,
                                    @City,
                                    @Locale,
                                    @PostalCode,
                                    @Cancelled
                                );";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql, new
            {
                Guid = Guid.NewGuid(),
                entity.AccountUid,
                entity.LocationUid,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                entity.Primary,
                entity.JoinedDateUtc,
                entity.CancelDateUtc,
                entity.FirstName,
                entity.LastName,
                entity.Address,
                entity.City,
                entity.Locale,
                entity.PostalCode,
                entity.Cancelled
            });

            var affectedRows = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(Guid gUid, DapperDbContext dbContext)
        {
            const string sql = "DELETE FROM member WHERE Guid = @gUid;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql, new
            {
                gUid = gUid
            });

            var affectedRows = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            return affectedRows > 0;
        }

        public async Task<bool> UpdateAsync(Guid gUid, Member entity, DapperDbContext dbContext)
        {
            if (entity == null) return false;

            const string sql = @"
                           UPDATE member
                              SET   AccountUid = @AccountUid,
                                    LocationUid = @LocationUid,
                                    UpdatedUtc = @UpdatedUtc,
                                    Primary = @Primary,
                                    JoinedDateUtc = @JoinedDateUtc,
                                    CancelDateUtc = @CancelDateUtc,
                                    FirstName = @FirstName,
                                    LastName = @LastName,
                                    Address = @Address,
                                    City = @City,
                                    Locale = @Locale,
                                    PostalCode = @PostalCode,
                                    Cancelled = @Cancelled
                            WHERE Guid = @gUid;";

            var builder = new SqlBuilder();
            var template = builder.AddTemplate(sql, new
            {
                entity.AccountUid,
                entity.LocationUid,
                UpdatedUtc = DateTime.UtcNow,
                entity.Primary,
                entity.JoinedDateUtc,
                entity.CancelDateUtc,
                entity.FirstName,
                entity.LastName,
                entity.Address,
                entity.City,
                entity.Locale,
                entity.PostalCode,
                entity.Cancelled,
                entity.Guid
            });

            var affectedRows = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
               .ConfigureAwait(false);

            return affectedRows > 0;
        }

        public async Task<IEnumerable<Member>> GetAllAsync(DapperDbContext dbContext)
        {
            IEnumerable<Member> members;

            const string sql = @"SELECT Uid,
                                        Guid,
                                        AccountUid,
                                        LocationUid,
                                        CreatedUtc,
                                        UpdatedUtc,
                                        'Primary',
                                        JoinedDateUtc,
                                        CancelDateUtc,
                                        FirstName,
                                        LastName,
                                        Address,
                                        City,
                                        Locale,
                                        PostalCode,
                                        Cancelled
                                   FROM member;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            members = await dbContext.Session.QueryAsync<Member>(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            return (members);
        }

        public async Task<Member> GetByIdAsync(Guid gUid, DapperDbContext dbContext)
        {
            Member member;

            const string sql = @"SELECT Uid,
                                        Guid,
                                        AccountUid,
                                        LocationUid,
                                        CreatedUtc,
                                        UpdatedUtc,
                                        'Primary',
                                        JoinedDateUtc,
                                        CancelDateUtc,
                                        FirstName,
                                        LastName,
                                        Address,
                                        City,
                                        Locale,
                                        PostalCode,
                                        Cancelled
                                   FROM member
                                  WHERE Guid = @Guid";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            builder.Where("Guid = @gUid", new
            {
                Guid = gUid
            });

            member = await dbContext.Session.QueryFirstOrDefaultAsync<Member>(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            return (member);
        }
    }
}
