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
        /// <summary>
        /// Adds a new member record to the database.
        /// </summary>
        /// <param name="entity">The member entity to be added.</param>
        /// <param name="dbContext">The database context containing the session and transaction.</param>
        /// <returns>True if the member was successfully inserted, false otherwise.</returns>
        public async Task<bool> AddAsync(Member entity, DapperDbContext dbContext)
        {
            try
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
            catch 
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes a member record from the database by its unique identifier.
        /// </summary>
        /// <param name="gUid">The unique identifier of the member to delete.</param>
        /// <param name="dbContext">The database context containing the session and transaction.</param>
        /// <returns>True if the member was successfully deleted, false otherwise.</returns>
        public async Task<bool> DeleteAsync(Guid gUid, DapperDbContext dbContext)
        {

            try
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
            catch 
            {
                throw;
            }
        }

        /// <summary>
        /// Updates an existing member record in the database.
        /// </summary>
        /// <param name="gUid">The unique identifier of the member to update.</param>
        /// <param name="entity">The member entity containing the updated information.</param>
        /// <param name="dbContext">The database context containing the session and transaction.</param>
        /// <returns>True if the member was successfully updated, false if entity is null or no rows were affected.</returns>
        public async Task<bool> UpdateAsync(Guid gUid, Member entity, DapperDbContext dbContext)
        {
            if (entity == null) return false;

            try
            {
                const string sql = @"
                           UPDATE member
                              SET   AccountUid = @AccountUid,
                                    LocationUid = @LocationUid,
                                    UpdatedUtc = @UpdatedUtc,
                                    'Primary' = @Primary,
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
                    gUid
                });

                var affectedRows = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
                   .ConfigureAwait(false);

                return affectedRows > 0;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves all member records from the database.
        /// </summary>
        /// <param name="dbContext">The database context containing the session and transaction.</param>
        /// <returns>Enumerable collection of Member entities with all member records from the database.</returns>
        public async Task<IEnumerable<Member>> GetAllAsync(DapperDbContext dbContext)
        {
            try
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
            catch 
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific member record from the database by its unique identifier.
        /// </summary>
        /// <param name="gUid">The unique identifier of the member to retrieve.</param>
        /// <param name="dbContext">The database context containing the session and transaction.</param>
        /// <returns>Member entity with matching identifier, or null if not found.</returns>
        public async Task<Member> GetByIdAsync(Guid gUid, DapperDbContext dbContext)
        {
            try
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

                builder.Where("m.Guid = @Guid", new
                {
                    Guid = gUid
                });

                member = await dbContext.Session.QueryFirstOrDefaultAsync<Member>(template.RawSql, template.Parameters, dbContext.Transaction)
                    .ConfigureAwait(false);

                return (member);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Validates whether a primary member already exists for a specific account.
        /// </summary>
        /// <param name="accountGuid">The unique identifier of the account.</param>
        /// <param name="dbContext">The database context containing the session and transaction.</param>
        /// <returns>True if a primary member exists for the account, false otherwise.</returns>
        public async Task<bool> ExistingPrimaryMemberByAccountValidation(Guid accountGuid, DapperDbContext dbContext)
        {
            try
            {
                var members = await GetAllMembersByAccountAsync(accountGuid, dbContext);

                return members.Any(m => m.Primary);
            }
            catch 
            {
                throw;
            }

        }

        /// <summary>
        /// Validates whether an account has more than one member.
        /// </summary>
        /// <param name="accountGuid">The unique identifier of the account.</param>
        /// <param name="dbContext">The database context containing the session and transaction.</param>
        /// <returns>True if the account has more than one member, false if it is the last member.</returns>
        public async Task<bool> LastAccountMemberValidation(Guid accountGuid, DapperDbContext dbContext)
        {
            try
            {
                var members = await GetAllMembersByAccountAsync(accountGuid, dbContext);

                return members.Count() > 1;
            }
            catch
            {
                throw;
            }

        }

        /// <summary>
        /// Retrieves all members belonging to a specific account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="dbContext">The database context containing the session and transaction.</param>
        /// <returns>Enumerable collection of Member entities for the specified account.</returns>
        public async Task<IEnumerable<Member>> GetAllMembersByAccountAsync(Guid accountId, DapperDbContext dbContext)
        {
            try
            {
                IEnumerable<Member> membersByAccount;

                const string sql = @"SELECT m.UID,
                                        m.Guid,
                                        a.Guid AS AccountGuid,
                                        l.Guid AS LocationGuid,
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
                            INNER  JOIN account a ON a.Uid = m.AccountUid
                            INNER  JOIN location l ON l.Uid = m.LocationUid
                                  WHERE a.Guid = @id;";

                var builder = new SqlBuilder();

                builder.Where("a.Guid = @id", new
                {
                    id = accountId
                });

                var template = builder.AddTemplate(sql);

                membersByAccount = await dbContext.Session.QueryAsync<Member>(template.RawSql, template.Parameters, dbContext.Transaction);

                return membersByAccount;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes all non-primary members associated with a specific account.
        /// </summary>
        /// <param name="gUid">The unique identifier of the account.</param>
        /// <param name="dbContext">The database context containing the session and transaction.</param>
        /// <returns>True if non-primary members were successfully deleted, false otherwise.</returns>
        public async Task<bool> DeleteNonPrimaryMembersAsyncByAccount(Guid gUid, DapperDbContext dbContext)
        {

            try
            {
                const string sql = @"DELETE FROM member WHERE Guid IN( 
                                    SELECT m.Guid
                                    FROM member m
                                    INNER  JOIN account a ON a.Uid = m.AccountUid
                                    WHERE a.Guid = @gUid AND m.'Primary' = 0
                               );";

                var builder = new SqlBuilder();

                var template = builder.AddTemplate(sql, new
                {
                    gUid = gUid
                });

                var affectedRows = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
                    .ConfigureAwait(false);

                return affectedRows > 0;
            }
            catch 
            {
                throw;
            }
        }
    }
}

