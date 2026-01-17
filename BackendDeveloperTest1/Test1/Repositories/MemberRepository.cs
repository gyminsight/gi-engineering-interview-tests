using Dapper;
using System;
using System.Threading;
using Test1.Contracts;
using Test1.Core;
using Test1.Interfaces;
using Test1.Models;

namespace Test1.Repositories
{
    public class MemberRepository : IMemberRepository
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

            const string sql = @"SELECT m.Uid,
                                        m.Guid,
                                        a.Guid AS AccountGuid,
                                        l.Guid AS LocationGuid,
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
                            INNER  JOIN account a ON a.Uid = m.AccountUid
                            INNER  JOIN location l ON l.Uid = m.LocationUid
                                   ;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            members = await dbContext.Session.QueryAsync<Member>(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            return (members);
        }

        public async Task<Member> GetByIdAsync(Guid gUid, DapperDbContext dbContext)
        {
            Member member;

            const string sql = @"SELECT m.Uid,
                                        m.Guid,
                                        a.Guid AS AccountGuid,
                                        l.Guid AS LocationGuid,
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
                            INNER  JOIN account a ON a.Uid = m.AccountUid
                            INNER  JOIN location l ON l.Uid = m.LocationUid
                                  WHERE m.Guid = @Guid";

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

        public async Task<bool> ExistingPrimaryMemberByAccountValidation(Guid accountGuid, DapperDbContext dbContext)
        {
            Member member;
            const string sql = @"SELECT m.Uid,
                                        m.Guid,
                                        a.Guid AS AccountGuid,
                                        m.'Primary'

                                    FROM member m
                              INNER JOIN account a ON a.Uid = m.AccountUid
                                   WHERE AccountGuid = @accountGuid AND m.'Primary' = 1";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            builder.Where("AccountGuid = @accountGuid", new
            {
                accountGuid = accountGuid
            });

            member = await dbContext.Session.QueryFirstOrDefaultAsync<Member>(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            return member != null;
        }

        public async Task<bool> LastAccountMemberValidation(Guid accountGuid, DapperDbContext dbContext)
        {
            Member member;
            const string sql = @"SELECT m.Uid,
                                        m.Guid,
                                        a.Guid AS AccountGuid

                                    FROM member m
                             INNER JOIN account a ON a.Uid = m.AccountUid
                                   WHERE AccountGuid = @accountGuid";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            builder.Where("AccountGuid = @accountGuid", new
            {
                accountGuid = accountGuid
            });

            member = await dbContext.Session.QueryFirstOrDefaultAsync<Member>(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            return member != null;
        }

        public async Task<IEnumerable<Member>> GetAllMembersByAccountAsync(Guid accountId, DapperDbContext dbContext)
        {
            IEnumerable<Member> membersByAccount;

            const string sql = @"SELECT m.UID,
                                        m.Guid,
                                        a.Guid AS AccountGuid,
                                        l.Guid AS LocationGuid,
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
                            INNER  JOIN account a ON a.Uid = m.AccountUid
                            INNER  JOIN location l ON l.Uid = m.LocationUid
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

